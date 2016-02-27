using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MonoDevelop.Projects;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class UnitTestHelper
	{
		string[] AllPossiblePostfixesForFileMatching;
		Dictionary<string,string> _statusTextsForFilePostFix = new Dictionary<string, string> {
			["Tests.cs" ] = "-> TEST",
			[".cs" ] = "-> IMPL",
		};

		static UnitTestHelper _instance;

		public static UnitTestHelper Instance {
			get {
				if (_instance == null) {
					_instance = new UnitTestHelper ();
				}
				return _instance;
			}
		}

		public UnitTestHelper ()
		{
			AllPossiblePostfixesForFileMatching = new string[]{ "Tests.cs", ".cs" };
		}

		#region methods that are virtual to facilitate unit testing

		internal virtual string CurrentFileName {
			get {
				return IdeApp.Workbench.ActiveDocument.FileName.FullPath.ToString ();
			}
		}

		internal virtual bool GetFileExists (string filename)
		{
			return File.Exists (filename);
		}

		#endregion

		public bool IsTogglingPossibleForActiveDocument {
			get {
				return GetFileExists (TestsFileNameForActiveDocument) &&
				GetFileExists (ImplementationFileNameForActiveDocument);
			}
		}

		public bool IsActiveFileUnitTestFile {
			get {
				return CurrentFileName.EndsWith ("Tests.cs");
			}
		}

		public bool IsActiveFileImplementationFile {
			get {
				return CurrentFileName.EndsWith (".cs");
			}
		}

		public string RootFileNameForActiveDocument {
			get {
				var file = CurrentFileName;
				foreach (var postfix in AllPossiblePostfixesForFileMatching) {
					if (file.EndsWith (postfix)) {
						return file.Replace (postfix, "");
					}
				}
				return null;
			}
		}

		public string TestsFileNameForActiveDocument {
			get {
				var rootFileName = GetRootFileNameForActiveDocument ();
				var project = GetProject (false);
				if (rootFileName.EndsWith (".cs")) {
					rootFileName = rootFileName.Replace (".cs", "Tests.cs");
				}
				var targetFileName = GetFileNameInProject (rootFileName, project);
				return targetFileName;
			}
		}

		public string ImplementationFileNameForActiveDocument {
			get {
				var rootFileName = GetRootFileNameForActiveDocument ();
				var project = GetProject (false);
				if (rootFileName.EndsWith ("Tests.cs")) {
					rootFileName = rootFileName.Replace ("Tests.cs", ".cs");
				}
				var targetFileName = GetFileNameInProject (rootFileName, project);
				return targetFileName;
				
			}
		}

		public void ToggleTestsAndImplementation ()
		{
			var entityInSourceDocument = MemberExtensionsHelper.Instance.GetEntityAtCaret ();
			entityInSourceDocument = entityInSourceDocument ?? MemberExtensionsHelper.Instance.GetNearestEntity (false);
			bool isMovingToUnitTestFile = !IsActiveFileUnitTestFile;
			//get the member in the current file
			var targetFilename = isMovingToUnitTestFile ? TestsFileNameForActiveDocument : ImplementationFileNameForActiveDocument;
			var project = GetProject (!IsActiveFileUnitTestFile);
			var success = OpenDocument (targetFilename, project);
			if (success) {
				
				if (entityInSourceDocument != null) {
					if (isMovingToUnitTestFile) {
						GotoImplementationMethodInUnitTestFile (entityInSourceDocument.Name);
					} else {
						GotoTestMethodInImplementationFile (entityInSourceDocument.Name);
					}
				}
			}
		}

		void GotoTestMethodInImplementationFile (string testMethodName)
		{
			var implementationMethodName = UnitTestHelper.GetMethodNameFromTestName (testMethodName);
			var entities = MemberExtensionsHelper.Instance.GetEntities ();
			//TODO make filter
			foreach (var e in entities) {
				if (e.Name.Contains (implementationMethodName)) {
					MemberExtensionsHelper.Instance.GotoMember (e);
				}
			}
		}

		void GotoImplementationMethodInUnitTestFile (string implementationMethodName)
		{
			var entities = MemberExtensionsHelper.Instance.GetEntities ();
			//TODO make filter
			foreach (var e in entities) {
				var possibleUnitTestNames = GetPossibleUnitTestNames (implementationMethodName);
				foreach (var possibleName in possibleUnitTestNames) {
					if (e.Name.Contains (possibleName)) {
						MemberExtensionsHelper.Instance.GotoMember (e);
					}
				}
			}
		}

		string[]GetPossibleUnitTestNames (string methodName)
		{
			return new string[] {
				methodName, 
				"Test" + methodName,
				"Test_" + methodName
			};
		}

		public string GetRootFileNameForActiveDocument ()
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

		string GetFileNameInProject (string filename, Project project)
		{
			var projectPath = project.GetAbsoluteChildPath ("").ToString ();
			var filePath = projectPath + filename;
			return filePath;
		}

		#region private impl

		Project GetProject (bool isUnitTestProject)
		{
			var projectName = IdeApp.Workbench.ActiveDocument.Project.Name;
			if (projectName.EndsWith ("Tests")) {
				projectName = projectName.Remove (projectName.Length - "Tests".Length);
			} else {
				projectName += "Tests";
			}
			var project = IdeApp.Workspace.GetAllProjects ().FirstOrDefault ((p) => p.Name == projectName);
			return project;
		}

		internal virtual bool OpenDocument (string filename, Project project)
		{
			if (!string.IsNullOrEmpty (filename)) {
				var filePath = new FilePath (filename);
				try {
					
					IdeHelper.OpenDocument (filePath, project);
					StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.OpenFileIcon, GetStatusPrefix (filename) + ": " + filename);
					return true;
				} catch (Exception ex) {
				}
			}
			StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.StatusError, "Could not find associated file");
			return false;
		}

		internal string GetStatusPrefix (string filename)
		{
			foreach (var key in _statusTextsForFilePostFix.Keys) {
				if (filename.EndsWith (key)) {
					return _statusTextsForFilePostFix [key];
				}
			}
			return null;
		}

		public static string GetMethodNameFromTestName (string text)
		{
			var pattern = @"(?<=(Test_|Test))[^_]([^_]*)";
			var match = Regex.Match (text, pattern);
			return match.Value;
		}


		#endregion


	}
}

