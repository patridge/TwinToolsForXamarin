using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{

	public class ToggleUnitTestAndImplementation : CommandHandler
	{
		protected override void Run ()
		{
			UnitTestHelper.Instance.ToggleTestsAndImplementation ();
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();
			isEnabled &= UnitTestHelper.Instance.IsTogglingPossibleForActiveDocument;
			isEnabled = true;
			info.Enabled = isEnabled;
			info.Visible = true;
		}
	}

}
