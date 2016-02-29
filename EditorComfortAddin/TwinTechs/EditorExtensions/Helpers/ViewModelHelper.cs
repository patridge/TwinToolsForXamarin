using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TwinTechs.EditorExtensions.Extensions;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class ViewModelHelper
	{
		string[] ViewModelSuffixes;
		string[] AllPossibleSuffixesForFileMatching;
		Dictionary<string,string> _statusTextsForFileSuffix = new Dictionary<string, string> {
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
			ViewModelSuffixes = new string[]{ "VM.cs", "ViewModel.cs" };
			AllPossibleSuffixesForFileMatching = new string[]{ ".xaml.cs", ".xaml", "VM.cs", "ViewModel.cs" };
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
				return VMFileNameForActiveDocument != null
				&& XamlFileNameForActiveDocument != null
				&& CodeBehindFileNameForActiveDocument != null;
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
				foreach (var viewModelSuffix in ViewModelSuffixes) {
					if (file.EndsWith (viewModelSuffix)) {
						return true;
					}
				}
				return false;
			}
		}

		public string RootFileNameForActiveDocument {
			get {
				var file = CurrentFileName;
				foreach (var suffix in AllPossibleSuffixesForFileMatching) {
					if (file.EndsWith (suffix)) {
						return file.Replace (suffix, "");
					}
				}
				return null;
			}
		}

		public string XamlFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;

				var possiblefileNames = GetPossibleFilenames (filename);
				foreach (var possibleFilename in possiblefileNames) {
					var targetFileName = possibleFilename + ".xaml";
					if (GetFileExists (targetFileName)) {
						return targetFileName;
					}
				}
				return null;
			}
		}

		public string VMFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;
				if (filename != null) {
					//it's possible that the filename is also in another place
					var possiblefileNames = GetPossibleFilenames (filename);
					foreach (var possibleFilename in possiblefileNames) {
						foreach (var viewModelSuffix in ViewModelSuffixes) {
							var targetFileName = possibleFilename + viewModelSuffix;
							if (GetFileExists (targetFileName)) {
								return targetFileName;
							}
						}
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Gets the possible folders, becuase we might have the viewmodels in the same folder, or
		/// in a viewmodel folder.
		/// </summary>
		/// <returns>The possible folders.</returns>
		/// <param name="folder">Folder.</param>
		public List<string> GetPossibleFilenames (string filename)
		{
			var folders = new List<string> ();
			if (filename != null) {
				
				folders.Add (filename);
				var pathName = System.IO.Path.GetDirectoryName (filename);
				var fileName = System.IO.Path.GetFileNameWithoutExtension (filename);
				if (pathName.EndsWith ("/View")) {
					var viewModelFolder = pathName.ReplaceLastOccurrence ("/View", "/ViewModel/");
					folders.Add (viewModelFolder + fileName);
				} else if (pathName.EndsWith ("/ViewModel")) {
					var viewModelFolder = pathName.ReplaceLastOccurrence ("/ViewModel", "/View/");
					folders.Add (viewModelFolder + fileName);
				}
			}
			return folders;
		}

		public string CodeBehindFileNameForActiveDocument {
			get {
				var filename = RootFileNameForActiveDocument;

				var possiblefileNames = GetPossibleFilenames (filename);
				foreach (var possibleFilename in possiblefileNames) {
					var targetFileName = possibleFilename + ".xaml.cs";
					if (GetFileExists (targetFileName)) {
						return targetFileName;
					}
				}
				return null;
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
			foreach (var key in _statusTextsForFileSuffix.Keys) {
				if (filename.EndsWith (key)) {
					return _statusTextsForFileSuffix [key];
				}
			}
			return null;
		}

		#endregion


	}
}

