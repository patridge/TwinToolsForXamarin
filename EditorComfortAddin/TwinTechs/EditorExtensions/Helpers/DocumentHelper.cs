using System;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Core;
using System.IO;

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

		public static string GetFileNameWithType (string fullTypeName, out Project targetProject)
		{
			targetProject = null;
			Solution currentSelectedSolution = IdeApp.ProjectOperations.CurrentSelectedSolution;
			if (currentSelectedSolution != null) {
				foreach (var project in currentSelectedSolution.GetAllProjects ()) {
					var dotNetProject = project as DotNetProject;
					LoggingService.LogDebug ("PROJECT>>>> " + dotNetProject.Name.ToString ());
					foreach (var s in dotNetProject.GetItemFiles (true)) {
						var path = GetRootFileNameForFileInProject (s.FullPath.ToString (), project);
						LoggingService.LogDebug (path);
						if (path.EndsWith (fullTypeName)) {
							targetProject = project;
							return s.FullPath.ToString ();
						}
					}
				}
			}
			return null;
		}

		public static string GetFileNameWithoutExtension (string path)
		{
			var returnPath = path;
			var fileName = Path.GetFileName (path);
			var firstDotIndex = fileName.IndexOf (".");
			if (firstDotIndex != -1) {
				var extensionLength = fileName.Length - firstDotIndex;
				returnPath = returnPath.Substring (0, returnPath.Length - extensionLength);
			}
			return returnPath;
		}

		public static string GetRootFileNameForFileInProject (string filename, Project project)
		{
			if (filename?.Length > 0) {
				var projectPath = project.GetAbsoluteChildPath ("").ToString ();
				if (projectPath?.Length < filename.Length) {
					var relativeFilePath = filename.ToString ().Substring (projectPath.Length);
					if (relativeFilePath.EndsWith (".cs")) {
						relativeFilePath = relativeFilePath.Replace (".cs", "");
					} else if (relativeFilePath.EndsWith (".xaml.cs")) {
						relativeFilePath = relativeFilePath.Replace (".xaml.cs", "");
					} else if (relativeFilePath.EndsWith (".xaml")) {
						relativeFilePath = relativeFilePath.Replace (".xaml", "");
					}
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
