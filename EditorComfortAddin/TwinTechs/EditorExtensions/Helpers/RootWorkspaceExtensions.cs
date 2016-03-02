using System;
using MonoDevelop.Ide;
using System.Linq;

namespace TwinTechs.EditorExtensions.Helpers
{
	public static class RootWorkspaceExtensions
	{
		public static bool GetIsWorkspaceOpen(this RootWorkspace workspace)
		{
			return workspace.GetAllSolutionItems().Any();
		}

		public static bool GetIsDocumentOpen(this RootWorkspace workspace)
		{
			return IdeApp.Workbench.ActiveDocument != null;
		}
	}
}

