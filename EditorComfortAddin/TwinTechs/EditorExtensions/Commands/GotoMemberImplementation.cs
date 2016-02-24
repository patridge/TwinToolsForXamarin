using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{
	/**
	 * Will navigate to the implementation of the member, instead of the interface
	 */
	public class GotoMemberImplementation : CommandHandler
	{
		protected override void Run ()
		{
			//1. identify if the member belongs to an interface
			var entity = MemberExtensionsHelper.Instance.GetEntityAtCaret ();
			//2. find classes that implement the interface
			//3. go to member in that class
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();

			info.Enabled = isEnabled;

			info.Visible = true;
		}
	}

}
