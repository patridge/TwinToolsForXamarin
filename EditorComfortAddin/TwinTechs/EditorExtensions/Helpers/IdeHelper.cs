using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace TwinTechs.EditorExtensions.Helpers
{
	public static class IdeHelper
	{

		/// <summary>
		/// Opens the document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="project">Project.</param>
		public static void OpenDocument (string fileName, Project project = null)
		{
			var projectToUse = project ?? IdeApp.Workbench.ActiveDocument.Project;
			IdeApp.Workbench.OpenDocument (fileName, projectToUse, OpenDocumentOptions.TryToReuseViewer | OpenDocumentOptions.BringToFront);
		}


	}
}


