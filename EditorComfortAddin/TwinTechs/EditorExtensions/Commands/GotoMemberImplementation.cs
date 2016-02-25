using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Core;
using MonoDevelop.Refactoring;
using MonoDevelop.Ide.Gui;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using TwinTechs.EditorExtensions.Helpers;
using MonoDevelop.Ide.FindInFiles;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using TwinTechs.EditorExtensions.Model;

namespace TwinTechs.EditorExtensions.Commands
{
	/**
	 * Will navigate to the implementation of the member, instead of the interface
	 */
	public class GotoMemberImplementation : CommandHandler
	{
		AbstractResolvedEntity _mostRecentEntity;
		DateTime _lastSearchTime;

		protected override void Run ()
		{
			//1. identify if the member belongs to an interface
			var entity = MemberExtensionsHelper.Instance.GetEntityAtCaret ();
			//2. find classes that implement the interface
			//3. go to member in that class

			Document activeDocument = IdeApp.Workbench.ActiveDocument;
			if (activeDocument != null && !(activeDocument.FileName == FilePath.Null)) {
				ResolveResult resolveResult;
				object item = CurrentRefactoryOperationsHandler.GetItem (activeDocument, out resolveResult);
				var resolvedEntity = item as AbstractResolvedEntity;
				if (resolvedEntity != null && resolvedEntity.DeclaringType.Kind == TypeKind.Interface) {
					_mostRecentEntity = null;
					NavigateToAbstractMember (resolvedEntity);
				} else if (IsRequestingCycleMostRecentMemberNavigation ()) {
					FindDerivedSymbolsHelper.CycleResults ();
				} else {
					_mostRecentEntity = null;
					NavigateToNonAbstractMember (resolvedEntity);
						
				}


			}

		}

		protected override void Update (CommandInfo info)
		{
			var isEnabled = IdeApp.Workspace.GetIsWorkspaceOpen () && IdeApp.Workspace.GetIsDocumentOpen ();

			info.Enabled = isEnabled;

			info.Visible = true;
		}

		void NavigateToNonAbstractMember (AbstractResolvedEntity entity)
		{
			//follow normal goto code path
			var namedElement = entity as INamedElement;
			if (namedElement != null) {
				IdeApp.ProjectOperations.JumpToDeclaration (namedElement, true);
			} else {
				IVariable variable = entity as IVariable;
				if (variable != null) {
					IdeApp.ProjectOperations.JumpToDeclaration (variable);
				}
			}
		}

		void NavigateToAbstractMember (AbstractResolvedEntity entity)
		{
			var member = entity as IMember;
			//if we already have a _mostRecentEntity and it's the same as the member we're on
			//then we just cycle the results
			_mostRecentEntity = entity;
			_lastSearchTime = DateTime.UtcNow;
			FindDerivedSymbolsHelper.FindDerivedMembers (member);
		}

		bool IsRequestingCycleMostRecentMemberNavigation ()
		{
			var entityAtCaret = MemberExtensionsHelper.Instance.GetEntityAtCaret ();
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			var currentLine = editor.Caret.Location.Line;
			return _mostRecentEntity != null && entityAtCaret.Name == _mostRecentEntity.Name && entityAtCaret.Region.Begin.Line == currentLine;
		}

	}
}
