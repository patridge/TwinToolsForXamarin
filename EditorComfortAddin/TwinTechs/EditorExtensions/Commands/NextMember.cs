using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{

	public class NextMember : CommandHandler
	{
		protected override void Run ()
		{
			MemberExtensionsHelper.Instance.GotoNextEntity ();
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();
			info.Enabled = isEnabled;
			info.Visible = true;
		}
	}

}
