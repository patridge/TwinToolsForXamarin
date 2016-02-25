using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using MonoDevelop.Refactoring;
using MonoDevelop.Ide.Gui;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class NavigationHelper
	{
		public NavigationHelper ()
		{
			
		}
	}
}

//
//		protected override async void Update (object ainfo)
//		{
//			var doc = IdeApp.Workbench.ActiveDocument;
//			if (doc == null || doc.FileName == FilePath.Null || doc.ParsedDocument == null)
//				return;
//
//			var semanticModel = doc.ParsedDocument.GetAst<SemanticModel> ();
//			if (semanticModel == null)
//				return;
//			var task = RefactoringSymbolInfo.GetSymbolInfoAsync (doc, doc.Editor);
//			if (!task.Wait (2000))
//				return;
//			var info = task.Result;
//			bool added = false;
//
//			var ext = doc.GetContent<CodeActionEditorExtension> ();
//
//			if (ext != null && !ext.GetCurrentFixes ().IsEmpty) {
//				var fixMenu = CreateFixMenu (doc.Editor, doc, ext.GetCurrentFixes ());
//				if (fixMenu.CommandInfos.Count > 0) {
//					ainfo.Add (fixMenu, null);
//					added = true;
//				}
//			}
//			var ciset = new CommandInfoSet ();
//			ciset.Text = GettextCatalog.GetString ("Refactor");
//
//			bool canRename = RenameHandler.CanRename (info.Symbol ?? info.DeclaredSymbol);
//			if (canRename) {
//				ciset.CommandInfos.Add (IdeApp.CommandService.GetCommandInfo (MonoDevelop.Ide.Commands.EditCommands.Rename), new Action (async delegate {
//					await new MonoDevelop.Refactoring.Rename.RenameRefactoring ().Rename (info.Symbol ?? info.DeclaredSymbol);
//				}));
//				added = true;
//			}
//			bool first = true;
//			if (ext != null) {
//				foreach (var fix in ext.GetCurrentFixes ().CodeRefactoringActions) {
//					if (added & first && ciset.CommandInfos.Count > 0)
//						ciset.CommandInfos.AddSeparator ();
//					var info2 = new CommandInfo (fix.CodeAction.Title);
//					ciset.CommandInfos.Add (info2, new Action (async () => await new CodeActionEditorExtension.ContextActionRunner (fix.CodeAction, doc.Editor, doc).Run ()));
//					added = true;
//					first = false;
//				}
//			}
//
//			if (ciset.CommandInfos.Count > 0) {
//				ainfo.Add (ciset, null);
//				added = true;
//			}
//
//			var gotoDeclarationSymbol = info.Symbol;
//			if (gotoDeclarationSymbol == null && info.DeclaredSymbol != null && info.DeclaredSymbol.Locations.Length > 1)
//				gotoDeclarationSymbol = info.DeclaredSymbol;
//			if (IdeApp.ProjectOperations.CanJumpToDeclaration (gotoDeclarationSymbol) || gotoDeclarationSymbol == null && IdeApp.ProjectOperations.CanJumpToDeclaration (info.CandidateSymbols.FirstOrDefault ())) {
//
//				var type = (gotoDeclarationSymbol ?? info.CandidateSymbols.FirstOrDefault ()) as INamedTypeSymbol;
//				if (type != null && type.Locations.Length > 1) {
//					var declSet = new CommandInfoSet ();
//					declSet.Text = GettextCatalog.GetString ("_Go to Declaration");
//					foreach (var part in type.Locations) {
//						var loc = part.GetLineSpan ();
//						declSet.CommandInfos.Add (string.Format (GettextCatalog.GetString ("{0}, Line {1}"), FormatFileName (part.SourceTree.FilePath), loc.StartLinePosition.Line + 1), new Action (() => IdeApp.ProjectOperations.JumpTo (type, part, doc.Project)));
//					}
//					ainfo.Add (declSet);
//				} else {
//					ainfo.Add (IdeApp.CommandService.GetCommandInfo (RefactoryCommands.GotoDeclaration), new Action (() => GotoDeclarationHandler.Run (doc)));
//				}
//				added = true;
//			}
//
//
//			if (info.DeclaredSymbol != null && GotoBaseDeclarationHandler.CanGotoBase (info.DeclaredSymbol)) {
//				ainfo.Add (GotoBaseDeclarationHandler.GetDescription (info.DeclaredSymbol), new Action (() => GotoBaseDeclarationHandler.GotoBase (doc, info.DeclaredSymbol)));
//				added = true;
//			}
//
//			var sym = info.Symbol ?? info.DeclaredSymbol;
//			if (doc.HasProject && sym != null) {
//				ainfo.Add (IdeApp.CommandService.GetCommandInfo (RefactoryCommands.FindReferences), new System.Action (() => {
//
//					if (sym.Kind == SymbolKind.Local || sym.Kind == SymbolKind.Parameter || sym.Kind == SymbolKind.TypeParameter) {
//						FindReferencesHandler.FindRefs (sym);
//					} else {
//						RefactoringService.FindReferencesAsync (sym.GetDocumentationCommentId ());
//					}
//
//				}));
//				try {
//					if (Microsoft.CodeAnalysis.FindSymbols.SymbolFinder.FindSimilarSymbols (sym, semanticModel.Compilation).Count () > 1)
//						ainfo.Add (IdeApp.CommandService.GetCommandInfo (RefactoryCommands.FindAllReferences), new System.Action (() => RefactoringService.FindAllReferencesAsync (sym.GetDocumentationCommentId ())));
//				} catch (Exception) {
//					// silently ignore roslyn bug.
//				}
//			}
//			added = true;
//
//		}
//
//		static string FormatFileName (string fileName)
//		{
//			if (fileName == null)
//				return null;
//			char[] seperators = { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
//			int idx = fileName.LastIndexOfAny (seperators);
//			if (idx > 0)
//				idx = fileName.LastIndexOfAny (seperators, idx - 1);
//			if (idx > 0)
//				return "..." + fileName.Substring (idx);
//			return fileName;
//		}

