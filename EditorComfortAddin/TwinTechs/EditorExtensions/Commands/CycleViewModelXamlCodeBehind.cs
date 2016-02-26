using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{

	public class CycleViewModelXamlCodeBehind : CommandHandler
	{
		protected override void Run ()
		{
			ViewModelHelper.Instance.CycleXamlCodeBehindViewModel ();
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();
			isEnabled &= ViewModelHelper.Instance.IsTogglingPossibleForActiveDocument;
			isEnabled &= ViewModelHelper.Instance.IsActiveFileAViewFile;

			//TODO check if document is xaml/codebehind/vm
			info.Enabled = isEnabled;
			info.Visible = true;
		}
	}

}
