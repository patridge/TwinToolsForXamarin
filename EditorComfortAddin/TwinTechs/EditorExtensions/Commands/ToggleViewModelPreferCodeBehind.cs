using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{

	public class ToggleViewModelPreferCodeBehind : CommandHandler
	{
		protected override void Run ()
		{
			MemberExtensionsHelper.Instance.ToggleVMXamlCs (true);
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();

			//TODO check if document is xaml/codebehind/vm
			info.Enabled = isEnabled;

			info.Visible = true;
		}
	}

}
