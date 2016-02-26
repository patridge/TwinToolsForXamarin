using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class ViewModelHelper
	{
		string[] ViewModelPostfixes;
		string[] AllPossiblePostfixesForFileMatching;
		Dictionary<string,string> _statusTextsForFilePostFix = new Dictionary<string, string> {
			[".xaml.cs" ] = "-> CB",
			[".xaml" ] = "-> XAML",
			["VM.cs" ] = "-> VM",
			["ViewModel.cs" ] = "-> VM",
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

		public ViewModelHelper ()
		{
			ViewModelPostfixes = new string[]{ "VM.cs", "ViewModel.cs" };
			AllPossiblePostfixesForFileMatching = new string[]{ ".xaml.cs", ".xaml", "VM.cs", "ViewModel.cs" };
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
				GetFileExists (XamlFileNameForActiveDocument) &&
				GetFileExists (CodeBehindFileNameForActiveDocument);
			}
		}

		public bool IsActiveFileAViewFile {
			get {
				return IsActiveFileCodeBehindFile || IsActiveFileXamlFile || IsActiveFileViewModel;
			}
		}

		public bool IsActiveFileXamlFile {
			get {
				return CurrentFileName.EndsWith (".xaml");
			}
		}

		public bool IsActiveFileCodeBehindFile {
			get {
				return CurrentFileName.EndsWith (".xaml.cs");
			}
		}

		public bool IsActiveFileViewModel {
			get {
				var file = CurrentFileName;
				foreach (var viewModelPostFix in ViewModelPostfixes) {
					if (file.EndsWith (viewModelPostFix)) {
						return true;
					}
				}
				return false;
			}
		}

		public string RootFileNameForActiveDocument {
			get {
				var file = CurrentFileName;
				foreach (var viewModelPostFix in AllPossiblePostfixesForFileMatching) {
					if (file.EndsWith (viewModelPostFix)) {
						return file.Replace (viewModelPostFix, "");
					}
				}
				return null;
			}
		}

		public string XamlFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;
				if (filename != null) {
					return filename + ".xaml";
				} else {
					return null;
				}
			}
		}

		public string VMFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;
				if (filename != null) {
					foreach (var viewModelPostFix in ViewModelPostfixes) {
						var targetFileName = filename + viewModelPostFix;
						if (GetFileExists (targetFileName)) {
							return targetFileName;
						}
					}
				}
				return null;
			}
		}

		public string CodeBehindFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;
				if (filename != null) {
					return filename + ".xaml.cs";
				} else {
					return null;
				}
				
			}
		}


		public void ToggleVMAndXaml ()
		{
			string filename = IsActiveFileXamlFile ? VMFileNameForActiveDocument : XamlFileNameForActiveDocument;
			OpenDocument (filename);
		}

		public void ToggleVMAndCodeBehind ()
		{
			string filename = IsActiveFileCodeBehindFile ? VMFileNameForActiveDocument : CodeBehindFileNameForActiveDocument;
			OpenDocument (filename);
		}

		public void CycleXamlCodeBehindViewModel ()
		{

			string filename = null;
			if (IsActiveFileXamlFile) {
				filename = CodeBehindFileNameForActiveDocument;
			} else if (IsActiveFileCodeBehindFile) {
				filename = VMFileNameForActiveDocument;
			} else {
				filename = XamlFileNameForActiveDocument;
			}
			OpenDocument (filename);
		}



		public void OpenXamlDocument (string fileName, bool gotoCodeBehind = false)
		{
		}

		public void OpenVMDocument ()
		{
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

