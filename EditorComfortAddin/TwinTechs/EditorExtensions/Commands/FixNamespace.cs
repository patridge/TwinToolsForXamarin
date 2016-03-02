using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using System.Text.RegularExpressions;
using System.IO;
using MonoDevelop.Projects;
using GLib;
using System.Collections.Generic;
using MonoDevelop.Ide.FindInFiles;
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using ICSharpCode.NRefactory.CSharp.Resolver;
using TwinTechs.EditorExtensions.Model;
using System.Diagnostics;
using ICSharpCode.NRefactory.MonoCSharp;
using Gtk;
using MonoDevelop.Ide.Editor;

namespace TwinTechs.EditorExtensions.Commands
{
	public class FixNamespace : CommandHandler
	{

		bool _isRunning;

		MonoDevelop.Ide.Gui.Document _document;



		string _originalNamespace;
		string _originalFullTypeName;
		string _newNamespace;
		string _newFullTypeName;

		List<string> _failedFiles;

		string _typeName;

		ICSharpCode.NRefactory.TypeSystem.ITypeDefinition _typeDefinition;

		DotNetProject _project;

		List<string> _filesToBeEdited;
		TextEditor _editor;

		protected override void Run()
		{
			_isRunning = true;
			try
			{
				_document = IdeApp.Workbench.ActiveDocument;
				_editor = _document.Editor;
				_project = _document.Project as DotNetProject;

				if (_project != null)
				{
					_newNamespace = _project.GetDefaultNamespace(_document.FileName.ToString());

					var namespaceLine = _editor.GetLine(_editor.CaretLine);
					var oldNamespacetext = _editor.GetLineText(_editor.CaretLine);
					_originalNamespace = GetNameSpaceAtCaret();
					var newNamespaceText = oldNamespacetext.Replace(_originalNamespace, _newNamespace);

					//TODO ask for confirmation
					var isRefactoring = ShowMessageBox("Do you wish to refactor?\nThe refactoring functionality is experimental, you should ensure you have committed/backed up first.");
					if (isRefactoring)
					{
						RefactorNamespaceChanges();
					}
					var modifiedText = GetUpdatedSourceText(_editor.Text);
					var oldNamespaceStatement = string.Format("namespace {0}", _originalNamespace);
					var newNameSpaceStatement = string.Format("namespace {0}", _newNamespace);

					modifiedText = modifiedText.Replace(oldNamespaceStatement, newNameSpaceStatement);
					_editor.Text = modifiedText;
					_editor.CaretLine = namespaceLine.LineNumber;
					_editor.CaretColumn = "namespace ".Length + 1;
					_document.IsDirty = true;
					//					_editor.Replace (namespaceLine.Offset, namespaceLine.Length, newNamespaceText);


				}
			}
			catch (Exception ex)
			{

			}
			_isRunning = false;
		}

		protected override void Update(CommandInfo info)
		{
			var isEnabled = !_isRunning && IdeApp.Workspace.GetIsWorkspaceOpen() && IdeApp.Workspace.GetIsDocumentOpen()
							&& GetNameSpaceAtCaret() != null;

			info.Enabled = isEnabled;
			info.Visible = true;
		}

		string GetNameSpaceAtCaret()
		{
			var pattern = @"(?<=(namespace))[^_]([^{]*)";
			var editor = IdeApp.Workbench.ActiveDocument.Editor;

			var nameSpaceMatch = Regex.Match(editor.GetLineText(editor.CaretLine), pattern);
			if (nameSpaceMatch.Success)
			{
				return nameSpaceMatch.ToString().Trim();
			}
			else {
				return null;
			}
		}

		#region experimental code to update namespaces in packages

		void RefactorNamespaceChanges()
		{
			GetFilesToUpdate();
			var isRefactoring = ShowMessageBox("Found " + _filesToBeEdited.Count + " Files. Continue?");
			if (isRefactoring)
			{
				_failedFiles = new List<string>();
				foreach (var fileName in _filesToBeEdited)
				{
					var openDocument = IdeApp.Workbench.Documents.FirstOrDefault(d => d.FileName.FullPath == fileName);
					if (openDocument == _document)
					{
						continue;
					}
					var isFileUpdateSuccesful = false;
					if (openDocument != null)
					{
						isFileUpdateSuccesful = EditNameSpacesInOpenDocument(openDocument);
					}
					else {
						isFileUpdateSuccesful = EditNameSpaceInFile(fileName);
					}
					if (!isFileUpdateSuccesful)
					{
						_failedFiles.Add(fileName);
					}
				}
			}
		}

		void GetFilesToUpdate()
		{
			_filesToBeEdited = new List<string>();
			if (!(_document.ParsedDocument.TopLevelTypeDefinitions?.Count > 0))
			{
				return;
			}
			var entities = _document.ParsedDocument.TopLevelTypeDefinitions;
			var topLevelClass = entities[0];
			_typeDefinition = topLevelClass.Resolve(_project) as ICSharpCode.NRefactory.TypeSystem.ITypeDefinition;

			_typeName = _typeDefinition.Name;
			_originalFullTypeName = _originalNamespace + "." + _typeName;
			_newFullTypeName = _newNamespace + "." + _typeName;

			var memberRefs = ReferenceFinder.FindReferences(_project.ParentSolution, _typeDefinition, true, ReferenceFinder.RefactoryScope.Unknown, null);
			foreach (var memberRef in memberRefs)
			{
				if (!(_filesToBeEdited).Contains(memberRef.FileName))
				{
					_filesToBeEdited.Add(memberRef.FileName);
				}
			}
		}


		bool EditNameSpaceInFile(string fileName)
		{
			try
			{
				var text = GetUpdatedSourceText(File.ReadAllText(fileName));
				File.WriteAllText(fileName, text);
				return true;
			}
			catch (Exception ex)
			{
				LoggingService.LogInternalError(ex);
				return false;
			}
		}

		bool EditNameSpacesInOpenDocument(MonoDevelop.Ide.Gui.Document document)
		{
			try
			{
				var text = GetUpdatedSourceText(document.Editor.Text);
				document.Editor.Text = text;
				document.IsDirty = true;
				return true;
			}
			catch (Exception ex)
			{
				LoggingService.LogInternalError(ex);
				return false;
			}
		}

		string GetUpdatedSourceText(string text)
		{
			if (_originalNamespace != _newNamespace)
			{
				var oldNamespaceStatement = string.Format("using {0};", _originalNamespace);
				//				var newNameSpaceStatement = string.Format ("{0}\nusing {1};\n", oldNamespaceStatement, _newNamespace);
				//TODO = in time we want to be able to check each document to see if has a depencency on anything else in this package!
				var newNameSpaceStatement = string.Format("using {0};", _newNamespace);
				text = Regex.Replace(text, oldNamespaceStatement, newNameSpaceStatement);

				text = Regex.Replace(text, _originalFullTypeName, _newFullTypeName);
			}
			return text;
		}

		bool ShowMessageBox(string message, MessageType messageType = MessageType.Question)
		{
			MessageDialog md = new MessageDialog(null, DialogFlags.Modal, messageType, ButtonsType.YesNo, message);
			var response = (ResponseType)md.Run();
			md.Destroy();
			return response == ResponseType.Yes;
		}

		#endregion
	}
}

