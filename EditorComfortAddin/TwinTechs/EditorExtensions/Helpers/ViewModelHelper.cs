using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using System.IO;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class ViewModelHelper
	{
		string[] ViewModelPostfixes;

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
		}


		/// <summary>
		/// Assumptions
		/// 1. the view model is described in the classes type specifier of xaml page
		/// 2. the viewmodel and xaml files will be in the same project
		/// TODO filter betwee
		/// x:TypeArguments="package.Class"
		/// </summary>
		public void ToggleVMXamlCs (bool preferCodeBehind = false)
		{
			var file = IdeApp.Workbench.ActiveDocument.FileName.FullPath.ToString ();
			var isXamlFile = file.Contains (".xaml");

			if (isXamlFile) {
				if (file.EndsWith (".xaml.cs")) {
					file = file.Replace (".xaml.cs", ".xaml");
				}
				OpenVMDocument (file);
				StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.OpenFileIcon, "-> VM:" + file);
			} else {

				OpenXamlDocument (file, preferCodeBehind);
			}
		}

		/// <summary>
		/// Opens the xaml document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="gotoCodeBehind">If set to <c>true</c> goto code behind.</param>
		public void OpenXamlDocument (string fileName, bool gotoCodeBehind = false)
		{

			//jump back to xaml file - do this based on the class containing vm or viewmodel extension

			foreach (var viewModelPostFix in ViewModelPostfixes) {
				if (fileName.Contains (viewModelPostFix)) {
					var fileExtension = gotoCodeBehind ? ".xaml.cs" : ".xaml";
					var targetFileName = fileName.Replace (viewModelPostFix, fileExtension);
					//for now assume in the same folder
					var filePath = new FilePath (targetFileName);
					IdeHelper.OpenDocument (filePath);
					StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.OpenFileIcon, (gotoCodeBehind ? "-> CB :" : "-> XAML :") + fileName);
					break;
				}
			}
		}

		/// <summary>
		/// Opens the VM document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		public void OpenVMDocument (string fileName)
		{
			foreach (var viewModelPostFix in new string[]{"VM.cs","ViewModel.cs"}) {
				var targetFileName = fileName.Replace (".xaml", viewModelPostFix);
				if (File.Exists (targetFileName)) {
					var filePath = new FilePath (targetFileName);
					//for now assume in the same folder
					IdeHelper.OpenDocument (filePath);
					break;
				}
			}
		}


	}
}

