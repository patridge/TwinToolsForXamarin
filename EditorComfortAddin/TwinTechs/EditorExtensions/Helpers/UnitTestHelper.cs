using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MonoDevelop.Projects;
using System.Linq;
using System.Text.RegularExpressions;
using MonoDevelop.Ide.Gui;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class UnitTestHelper
	{
		string TestClassTemplate = "using System;\nusing NUnit.Framework;\n\nnamespace NAMESPACE\n{\n\t[TestFixture]\n\tpublic class CLASSNAME\n\t{\n\t\t[SetUp]\n\t\tpublic void Setup ()\n\t\t{\n\t\t}\n\n\t\t[TearDown]\n\t\tpublic void TearDown ()\n\t\t{\n\t\t}\n\t}\n}\n\n";

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
				var project = GetProject (true);
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

		public bool GotoTestMethodInImplementationFile (string testMethodName)
		{
			var implementationMethodName = UnitTestHelper.GetMethodNameFromTestName (testMethodName);
			if (implementationMethodName == "Constructor") {
				implementationMethodName = ".ctor";
			}
			var entities = MemberExtensionsHelper.Instance.GetEntities ();
			//TODO make filter
			foreach (var e in entities) {
				if (e.Name.Contains (implementationMethodName)) {
					MemberExtensionsHelper.Instance.GotoMember (e);
					return true;
				}
			}
			return false;
		}

		public bool GotoImplementationMethodInUnitTestFile (string implementationMethodName)
		{
			var entities = MemberExtensionsHelper.Instance.GetEntities ();
			//TODO make filter
			foreach (var e in entities) {
				var possibleUnitTestNames = GetPossibleUnitTestNames (implementationMethodName);
				foreach (var possibleName in possibleUnitTestNames) {
					if (e.Name.Contains (possibleName)) {
						MemberExtensionsHelper.Instance.GotoMember (e);
						return true;
					}
				}
			}
			return false;
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
					relativeFilePath = relativeFilePath.Replace ("Tests.cs", ".cs");
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



		public Project GetProject (bool isUnitTestProject)
		{
			var projectName = IdeApp.Workbench.ActiveDocument.Project.Name;
			if (isUnitTestProject) {
				if (!projectName.EndsWith ("Tests")) {
					projectName += "Tests";
				}
			} else {
				if (projectName.EndsWith ("Tests")) {
					projectName = projectName.Remove (projectName.Length - "Tests".Length);
				}
			}

			var project = IdeApp.Workspace.GetAllProjects ().FirstOrDefault ((p) => p.Name == projectName);
			return project;
		}



		public virtual bool OpenDocument (string filename, Project project)
		{
			if (!string.IsNullOrEmpty (filename) && File.Exists (filename)) {
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

		#region project level methods

		public bool CreateTestFile (Project unitTestProject, string fileLocation)
		{
			var implementationClassName = System.IO.Path.GetFileNameWithoutExtension (ImplementationFileNameForActiveDocument);
			var declaringType = MemberExtensionsHelper.Instance.GetDeclaringTypeWithName (implementationClassName);
			if (declaringType == null) {
				return false;
			}
			var unitTestClassName = System.IO.Path.GetFileNameWithoutExtension (TestsFileNameForActiveDocument);
			var pathName = System.IO.Path.GetDirectoryName (fileLocation);

			//TODO get root namespace of both impl/test projects, if required and set accordingly
			var nameSpace = "";
			nameSpace += declaringType.Namespace;
			var contents = TestClassTemplate.Replace ("NAMESPACE", nameSpace);
			contents = contents.Replace ("CLASSNAME", unitTestClassName);
			try {
				if (!Directory.Exists (pathName)) {
					Directory.CreateDirectory (pathName);
				}
				System.IO.File.WriteAllText (fileLocation, contents);
				unitTestProject.AddFile (fileLocation);
				IdeApp.ProjectOperations.Save (unitTestProject);
				return true;
			} catch (Exception ex) {
				Console.WriteLine ("error creating unit test");
				StatusHelper.ShowStatus (Stock.StatusError, "Couldn't create unit test file");
				return false;
			}

		}

		public bool CreateTestProject ()
		{
			//TODO not yet supported
			StatusHelper.ShowStatus (Stock.StatusError, "Please create a unit test project and try again");
			return false;
		}

		#endregion


		#region private impl

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

