using System;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using TwinTechs.EditorExtensions.Helpers;
using MonoDevelop.Ide.FindInFiles;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using TwinTechs.EditorExtensions.Model;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Text;
using Mono.TextEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using MonoDevelop.Projects;
using MonoDevelop.Ide.TypeSystem;
using System.Threading.Tasks;
using MonoDevelop.Components.MainToolbar;

[assembly: InternalsVisibleTo("ComfortAddInTests")]
namespace TwinTechs.EditorExtensions.Commands
{
	/**
	 * Will navigate to the implementation of the member, instead of the interface
	 */
	public class GoToDeclarationPlus : CommandHandler
	{
		AbstractResolvedEntity _mostRecentEntity;
		DateTime _lastSearchTime;

		static MemberReference _currentMemberReference;
		static List<MemberReference> _foundMemberReferences;
		static bool _didFindFirstResult;


		protected override void Run()
		{
			//1. identify if the member belongs to an interface
			var entity = MemberExtensionsHelper.Instance.GetEntityAtCaret();
			//2. find classes that implement the interface
			//3. go to member in that class

			Document activeDocument = IdeApp.Workbench.ActiveDocument;
			if (activeDocument != null && !(activeDocument.FileName == FilePath.Null))
			{
				ResolveResult resolveResult;
				object item = CurrentRefactoryOperationsHandler.GetItem(activeDocument, out resolveResult);
				var resolvedEntity = item as AbstractResolvedEntity;
				if (resolvedEntity != null && resolvedEntity.DeclaringType.Kind == TypeKind.Interface)
				{
					NavigateToAbstractMember(resolvedEntity);
				}
				else if (IsRequestingCycleMostRecentMemberNavigation())
				{
					CycleResults();
				}
				else if (ViewModelHelper.Instance.IsActiveFileXamlFile)
				{
					_mostRecentEntity = null;
					NavigateToXamlValue(resolvedEntity);
				}
				else {
					_mostRecentEntity = null;
					NavigateToNonAbstractMember(item);

				}

			}

		}

		protected override void Update(CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen() && IdeApp.Workspace.GetIsDocumentOpen();

			info.Enabled = isEnabled;

			info.Visible = true;
		}

		void NavigateToNonAbstractMember(object entity)
		{
			//follow normal goto code path
			var namedElement = entity as INamedElement;
			if (namedElement != null)
			{
				//FIXME = how to get reference to microsoft code analysis
				IdeApp.ProjectOperations.JumpToDeclaration(namedElement, true);
			}
			else {
				IVariable variable = entity as IVariable;
				if (variable != null)
				{
					//FIXME = how to get reference to microsoft code analysis
					IdeApp.ProjectOperations.JumpToDeclaration(variable);
				}
			}
		}

		void NavigateToAbstractMember(AbstractResolvedEntity entity)
		{
			ResetResults();
			var member = entity as IMember;
			//if we already have a _mostRecentEntity and it's the same as the member we're on
			//then we just cycle the results
			_mostRecentEntity = entity;
			_lastSearchTime = DateTime.UtcNow;
			FindDerivedSymbolsHelper.FindDerivedMembers(member, ReportResult);
		}

		void NavigateToXamlValue(AbstractResolvedEntity entity)
		{
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			//first try to go to the element
			//get text at caret, and expand out till we get soem quotes
			var line = IdeApp.Workbench.ActiveDocument.Editor.CaretLine;
			var column = IdeApp.Workbench.ActiveDocument.Editor.CaretColumn;
			var text = IdeApp.Workbench.ActiveDocument.Editor.GetLineText(line);

			var member = MemberExtensionsHelper.Instance.GetNearestEntity(false, true);
			//TODO get the exact property
			//var memberText = editor.GetTextBetween (member.Region.Begin, member.Region.End);

			//try to work out what this is..
			//			var didPass = XamlHelper.PerformXamlNavigationActionUsingText (editor.Text);
			var isPropertyValue = XamlHelper.GetIsPropertyValue(text, column);
			var valueText = XamlHelper.GetWordAtColumn(text, column);
			if (isPropertyValue)
			{
				if (!string.IsNullOrEmpty(valueText))
				{
					//crude mechanism for now to work out if this is a binding statemnet or not
					string fileName = null;
					if (text.Contains("{"))
					{
						//consider that it's a property on the vm
						//TODO make more robust.. check before opening
						fileName = ViewModelHelper.Instance.VMFileNameForActiveDocument;
					}
					else {
						//consider that it's a handler on the code behind
						//TODO make more robust.. check before opening
						fileName = ViewModelHelper.Instance.CodeBehindFileNameForActiveDocument;
					}
					ViewModelHelper.Instance.OpenDocument(fileName);
					//now go to the member
					MemberExtensionsHelper.Instance.GotoMemberWithName(valueText);
				}
			}
			else {
				var unresolvedMember = member as AbstractUnresolvedMember;

				if (unresolvedMember?.ReturnType != null)
				{
					var returnType = unresolvedMember.ReturnType as GetClassTypeReference;
					var fileNameString = returnType.FullTypeName.ReflectionName.Replace(".", "/");
					Project targetProject;
					var fileName = DocumentHelper.GetFileNameWithType(fileNameString, out targetProject);

					if (fileName != null)
					{
						IdeHelper.OpenDocument(fileName, targetProject);
						if (!string.IsNullOrEmpty(valueText))
						{
							MemberExtensionsHelper.Instance.GotoMemberWithName(valueText);
						}
					}
				}

			}
		}


		#region methods related to handling interface method cycling


		bool IsRequestingCycleMostRecentMemberNavigation()
		{
			var entityAtCaret = MemberExtensionsHelper.Instance.GetEntityAtCaret();
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			var currentLine = editor.CaretLocation.Line;
			return _mostRecentEntity != null && entityAtCaret != null && entityAtCaret.Name == _mostRecentEntity.Name && entityAtCaret.Region.Begin.Line == currentLine;
		}


		void CycleResults()
		{
			if (_foundMemberReferences.Count > 1)
			{
				var currentIndex = _foundMemberReferences.IndexOf(_currentMemberReference);
				currentIndex = currentIndex == _foundMemberReferences.Count - 1 ? 0 : currentIndex + 1;
				_currentMemberReference = _foundMemberReferences[currentIndex];
				NavigateToCurrentDocument();
			}
		}

		void ResetResults()
		{
			_mostRecentEntity = null;
			_currentMemberReference = null;
			_foundMemberReferences = new List<MemberReference>();
			_didFindFirstResult = false;
		}

		void NavigateToCurrentDocument()
		{
			Gtk.Application.Invoke(delegate
			{
				var filePath = new FilePath(_currentMemberReference.FileName);
				//TODO don't store member references; but store our own data-type to track these
				IdeApp.Workbench.OpenDocument(filePath, null, _currentMemberReference.Region.BeginLine, _currentMemberReference.Region.BeginColumn, OpenDocumentOptions.Default);
			});
			StatusHelper.ShowStatus(MonoDevelop.Ide.Gui.Stock.OpenFileIcon, GetStatusText());
		}

		string GetStatusText()
		{
			var message = GetNameWithoutLastBit(_currentMemberReference.FileName);
			if (_foundMemberReferences?.Count > 1)
			{
				var currentIndex = _foundMemberReferences.IndexOf(_currentMemberReference);
				message += "(" + (currentIndex + 1) + "/" + _foundMemberReferences.Count + ")";
			}
			return message;
		}

		string GetNameWithoutLastBit(string name)
		{
			int lastDotPosition = name.LastIndexOf(".");
			if (lastDotPosition >= 0)
			{
				name = name.Substring(0, lastDotPosition);
			}
			return name;
		}


		private void ReportResult(ISearchProgressMonitor monitor, IEntity result)
		{
			string fileName = result.Region.FileName;
			if (!string.IsNullOrEmpty(fileName))
			{
				var filePath = new FilePath(fileName);
				var textEditorData = TextFileProvider.Instance.GetTextEditorData(filePath);
				int num = textEditorData.LocationToOffset(result.Region.Begin.Line, result.Region.Begin.Column);

				//FIXME understand what this used to do, and if we need to even do this
				//might just be an artifact I picked up
				textEditorData.SearchRequest.SearchPattern = result.Name;
				Mono.TextEditor.SearchResult searchResult = textEditorData.SearchForward(num);
				if (searchResult != null)
				{
					num = searchResult.Offset;
				}
				if (textEditorData.Parent == null)
				{
					textEditorData.Dispose();
				}


				//FIXME - store in new object that can also store the region (perhaps extend member reference?
				var memberReference = new MemberReference(result, fileName, num, result.Name.Length);
				_foundMemberReferences.Add(memberReference);

				if (!_didFindFirstResult)
				{
					_didFindFirstResult = true;
					_currentMemberReference = memberReference;
					NavigateToCurrentDocument();
				}
				else {
					StatusHelper.ShowStatus(MonoDevelop.Ide.Gui.Stock.OpenFileIcon, GetStatusText());
				}

			}
		}

		#endregion

	}
}
