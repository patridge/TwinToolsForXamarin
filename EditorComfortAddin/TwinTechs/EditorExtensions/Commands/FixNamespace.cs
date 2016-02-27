using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using System.Text.RegularExpressions;
using System.IO;
using MonoDevelop.Projects;

namespace TwinTechs.EditorExtensions.Commands
{
	public class FixNamespace: CommandHandler
	{
		protected override void Run ()
		{
			var document = IdeApp.Workbench.ActiveDocument;
			var editor = document.Editor;
			var project = document.Project as DotNetProject;
			if (project != null) {
				var nameSpaceText = project.GetDefaultNamespace (document.FileName.ToString ());

				var namespaceLine = editor.GetLine (editor.Caret.Line);
				var oldNamespacetext = editor.GetLineText (editor.Caret.Line);
				var oldNamespace = GetNameSpaceAtCaret ();
				var newNamespaceText = oldNamespacetext.Replace (oldNamespace, nameSpaceText);
				editor.Replace (namespaceLine.Offset, namespaceLine.Length, newNamespaceText);
			}
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ()
			                && GetNameSpaceAtCaret () != null;

			info.Enabled = isEnabled;
			info.Visible = true;
		}

		string GetNameSpaceAtCaret ()
		{
			var document = IdeApp.Workbench.ActiveDocument;
			var editor = document.Editor;

			var pattern = @"(?<=(namespace))[^_]([^{]*)";
			var nameSpaceMatch = Regex.Match (editor.GetLineText (editor.Caret.Line), pattern);
			if (nameSpaceMatch.Success) {
				return nameSpaceMatch.ToString ().Trim ();
			} else {
				return null;
			}
		}
	}
}

