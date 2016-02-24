using System;
using MonoDevelop.Ide;

namespace TwinTechs.EditorExtensions.Helpers
{
	public static class RootWorkspaceExtensions
	{
		public static bool GetIsWorkspaceOpen (this RootWorkspace workspace)
		{
			return workspace.GetAllSolutions ().Count > 0;
		}

		public static bool GetIsDocumentOpen (this RootWorkspace workspace)
		{
			return IdeApp.Workbench.ActiveDocument != null;
		}
	}
}

