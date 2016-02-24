using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.View;

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
			info.Enabled = true;
			info.Visible = true;
		}
	}

}

