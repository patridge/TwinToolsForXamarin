using System;
using MonoDevelop.Ide;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class DocumentHelper
	{
		public DocumentHelper ()
		{
		}

		public static string GetRootFileNameForActiveDocument ()
		{
			var project = IdeApp.Workbench.ActiveDocument.Project;
			var filename = IdeApp.Workbench.ActiveDocument.FileName;
			var filePath = filename.FullPath.ToString ();
			if (filePath?.Length > 0) {
				var projectPath = project.GetAbsoluteChildPath ("").ToString ();
				if (projectPath?.Length < filePath.Length) {
					var relativeFilePath = filename.ToString ().Substring (projectPath.Length);
					return relativeFilePath;
				} else {
					return null;
				}
			} else {
				return null;

			}
		}
	}
}

