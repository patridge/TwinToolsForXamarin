using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.View;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{
	public class ShowMembers : CommandHandler
	{
		protected override void Run ()
		{
			if (IdeApp.Workbench?.ActiveDocument != null) {
				var memberListWindow = new MemberListWindow ("Members", IdeApp.Workbench.RootWindow, Gtk.DialogFlags.Modal);
				memberListWindow.Run ();
			}
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();

			info.Enabled = isEnabled;
			info.Visible = true;
		}
	}

}
