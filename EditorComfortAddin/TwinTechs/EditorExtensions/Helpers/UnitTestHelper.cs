using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class UnitTestHelper
	{
		string[] AllPossiblePostfixesForFileMatching;
		Dictionary<string,string> _statusTextsForFilePostFix = new Dictionary<string, string> {
			["Tests.cs" ] = "-> TEST",
			[".cs" ] = "-> IMPL",
		};

		static ViewModelHelper _instance;

		public static ViewModelHelper Instance {
			get {
				if (_instance == null) {
					_instance = new ViewModelHelper ();
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
				return VMFileNameForActiveDocument != null &&
				GetFileExists (TestsFileNameForActiveDocument) &&
				GetFileExists (ImplementationFileNameForActiveDocument);
			}
		}

		public bool IsActiveFileUnitTestFile {
			get {
				return CurrentFileName.EndsWith ("Tests.cs");
			}
		}

		public bool IsActiveFileImplementationFIle {
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
				var filename = RootFileNameForActiveDocument;
				if (filename != null) {
					return filename + "Tests.cs";
				} else {
					return null;
				}
			}
		}

		public string ImplementationFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;
				if (filename != null) {
					return filename + ".cs";
				} else {
					return null;
				}
				
			}
		}


		public void ToggleVMAndXaml ()
		{
			string filename = IsActiveFileUnitTestFile ? VMFileNameForActiveDocument : TestsFileNameForActiveDocument;
			OpenDocument (filename);
		}

		public void ToggleVMAndCodeBehind ()
		{
			string filename = IsActiveFileImplementationFIle ? VMFileNameForActiveDocument : ImplementationFileNameForActiveDocument;
			OpenDocument (filename);
		}

		#region private impl

		internal virtual void OpenDocument (string filename)
		{
			if (!string.IsNullOrEmpty (filename)) {
				var filePath = new FilePath (filename);
				IdeHelper.OpenDocument (filePath);
				StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.OpenFileIcon, GetStatusPrefix (filename) + ": " + filename);
			} else {
				StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.StatusError, "Could not find associated file");
			}
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

		#endregion


	}
}

