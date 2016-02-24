using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.View;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{
	public class ShowRecentDocumentBrowser : CommandHandler
	{
		protected override void Run ()
		{
			if (IdeApp.Workspace.IsOpen) {
				var recentDocumentsWindow = new RecentFileListWindow ("Browse Recent Documents", IdeApp.Workbench.RootWindow, Gtk.DialogFlags.Modal);
				recentDocumentsWindow.Run ();
			}
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen ();
			info.Enabled = isEnabled;
			info.Visible = true;
		}
	}

}

