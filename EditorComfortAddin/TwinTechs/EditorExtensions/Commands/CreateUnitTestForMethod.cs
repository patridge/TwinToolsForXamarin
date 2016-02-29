using System;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;
using System.IO;
using Mono.CSharp;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui;
using Mono.TextEditor.PopupWindow;
using System.Threading.Tasks;
using System.Threading;
using Gtk;
using System.Collections.Generic;
using Mono.TextEditor;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.Core;
using ICSharpCode.NRefactory.TypeSystem;

namespace TwinTechs.EditorExtensions.Commands
{
	public class CreateUnitTestForMethod : CommandHandler
	{
		
		string TestMethodTemplate = "[Test]\n\t\tpublic void METHODNAME ()\n\t\t{\n\t\t\tAssert.Fail (\"Implement me\");\n\t\t}";

		protected override void Run ()
		{
			var helper = UnitTestHelper.Instance;
			var unitTestProject = helper.GetProject (true);
			var isTestProjectCreated = unitTestProject != null;
			if (!isTestProjectCreated) {
				//ask to create project
				isTestProjectCreated = helper.CreateTestProject ();
				if (!isTestProjectCreated) {
					StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.StatusError, "Couldn't open unit test project");
					return;
				}
			}

			var entityInSourceDocument = MemberExtensionsHelper.Instance.GetEntityAtCaret ();
			entityInSourceDocument = entityInSourceDocument ?? MemberExtensionsHelper.Instance.GetNearestEntity (false);

			var testFileName = helper.TestsFileNameForActiveDocument;
			var methodName = entityInSourceDocument.Name;
			var isTestFilePresent = File.Exists (testFileName);
			if (!isTestFilePresent) {
				//ask to create document
				helper.CreateTestFile (unitTestProject, testFileName);
			}
			var success = helper.OpenDocument (testFileName, unitTestProject);
			if (success) {
				//attempt to navigate to method
				var didMethodExist = UnitTestHelper.Instance.GotoImplementationMethodInUnitTestFile (methodName);
				if (!didMethodExist) {
					var className = System.IO.Path.GetFileNameWithoutExtension (testFileName);
					if (methodName == ".ctor") {
						methodName = "Constructor";
					}
					CreateTestMethod (className, methodName);

				}
			} else {
				StatusHelper.ShowStatus (MonoDevelop.Ide.Gui.Stock.StatusError, "Couldn't open unit test file");
			}
		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ()
			                && !IdeApp.Workbench.ActiveDocument.FileName.ToString ().EndsWith (".xaml.cs")
			                && !IdeApp.Workbench.ActiveDocument.FileName.ToString ().EndsWith (".xaml")
			                && UnitTestHelper.Instance.IsActiveFileImplementationFile;

			var entityInSourceDocument = MemberExtensionsHelper.Instance.GetEntityAtCaret ();
			entityInSourceDocument = entityInSourceDocument ?? MemberExtensionsHelper.Instance.GetNearestEntity (false);

			isEnabled &= entityInSourceDocument != null;
			info.Enabled = isEnabled;
			info.Visible = true;
		}

		void CreateTestMethod (string className, string methodName)
		{
			
			var methodText = TestMethodTemplate.Replace ("METHODNAME", "Test_" + methodName);
			var document = IdeApp.Workbench.ActiveDocument;
			var editor = document.Editor;

			var declaringType = MemberExtensionsHelper.Instance.GetDeclaringTypeWithName (className);
			if (declaringType == null) {
				return;
			}
			var mode = new InsertionCursorEditMode (
				           editor.Parent,
				           CodeGenerationService.GetInsertionPoints (document, declaringType));
			if (mode.InsertionPoints.Count == 0) {
				MessageService.ShowError (
					GettextCatalog.GetString ("No valid insertion point can be found in type '{0}'.", declaringType.Name)
				);
				return;
			}
			var helpWindow = new Mono.TextEditor.PopupWindow.InsertionCursorLayoutModeHelpWindow ();
//				helpWindow.Window = MonoDevelop.Ide.IdeApp.Workbench.RootWindow;
			helpWindow.TitleText = "Insert test method";
			mode.HelpWindow = helpWindow;

			mode.CurIndex = 0;
			mode.StartMode ();
			mode.Exited += delegate(object s, InsertionCursorEventArgs iCArgs) {
				if (iCArgs.Success) {
					iCArgs.InsertionPoint.Insert (document.Editor, methodText);

					editor.SetCaretTo (iCArgs.InsertionPoint.Location.Line, iCArgs.InsertionPoint.Location.Column, true, false);

				}
			};
		}

	}
}