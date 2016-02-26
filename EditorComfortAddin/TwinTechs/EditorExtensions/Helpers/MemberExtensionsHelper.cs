using System;
using MonoDevelop.Ide.Gui;
using ICSharpCode.NRefactory.TypeSystem;
using System.Collections.ObjectModel;
using MonoDevelop.Ide;
using System.Linq;
using Mono.TextEditor;
using MonoDevelop.Core;
using System.IO;
using Mono.CSharp;

namespace TwinTechs.EditorExtensions.Helpers
{

	public class MemberExtensionsHelper
	{


		static MemberExtensionsHelper _instance;
		Document _mostRecentDocument;

		internal bool IsDirty { get; set; }

		Collection<IUnresolvedEntity> _cachedEntities;

		public static MemberExtensionsHelper Instance {
			get {
				if (_instance == null) {
					_instance = new MemberExtensionsHelper ();
				}
				return _instance;
			}
		}

		#region public api

		DateTime _lastUpdateTime;

		/// <summary>
		/// Gets the entities.
		/// </summary>
		/// <returns>The entities.</returns>
		public Collection<IUnresolvedEntity> GetEntities ()
		{
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			var parsedDoc = IdeApp.Workbench.ActiveDocument.ParsedDocument;
			if (_mostRecentDocument != IdeApp.Workbench.ActiveDocument) {
				_mostRecentDocument = IdeApp.Workbench.ActiveDocument;
				_mostRecentDocument.UpdateParseDocument ();
				IsDirty = true;
			}
			if (_lastUpdateTime == null) {
				IsDirty = true;
			} else {
				//TODO would be nice to get real dirty flag on the docs editor
				var now = DateTime.Now;
				var secondsSinceLastInvocation = (now - _lastUpdateTime).TotalSeconds;
				if (secondsSinceLastInvocation > 2) {
					IsDirty = true;
				}
			}

			if (IsDirty || _cachedEntities == null) {
				IdeApp.Workbench.ActiveDocument.UpdateParseDocument ();
				parsedDoc = IdeApp.Workbench.ActiveDocument.ParsedDocument;
				_cachedEntities = new Collection<IUnresolvedEntity> ();
				foreach (var typeDef in parsedDoc.TopLevelTypeDefinitions) {
					//TODO pretty print these
					_cachedEntities.Add (typeDef);
					foreach (var member in typeDef.Members) {
						_cachedEntities.Add (member);
					}
				}
				_lastUpdateTime = DateTime.Now;
				IsDirty = false;
			}

			return _cachedEntities;
		}

		/// <summary>
		/// Gotos the member.
		/// </summary>
		/// <param name="selectedMember">Selected member.</param>
		public void GotoMember (object selectedMember)
		{
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			var member = selectedMember as IUnresolvedEntity;
			if (member != null) {
				var memberType = selectedMember as IUnresolvedEntity;
				var region = memberType.Region;
				editor.SetCaretTo (region.BeginLine, region.BeginColumn, true, false);
				editor.CenterToCaret ();
			}
		}

		public void GotoMemberWithName (string memberName)
		{
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			var entities = GetEntities ();
			foreach (var entity in entities) {
				if (entity.Name.EndsWith (memberName)) {
					var region = entity.Region;
					editor.SetCaretTo (region.BeginLine, region.BeginColumn, true, false);
					editor.CenterToCaret ();	
				}
			}
		}

		/// <summary>
		/// Gets the entity at caret.
		/// </summary>
		/// <returns>The entity at caret.</returns>
		public IUnresolvedEntity GetEntityAtCaret ()
		{
			var editor = IdeApp.Workbench.ActiveDocument.Editor;

			//TODO cache these bad boys
			var entities = GetEntities ();
			foreach (var entity in entities) {
				if (!(entity is IUnresolvedTypeDefinition)) {
					if (entity.Region.Contains (editor.Caret.Location)) {
						return entity;
					}
				}
			}
			return null;

		}

		/// <summary>
		/// Gets the nearest entity.
		/// </summary>
		/// <returns>The nearest entity.</returns>
		/// <param name="isDirectionDown">If set to <c>true</c> is direction down.</param>
		public IUnresolvedEntity GetNearestEntity (bool isDirectionDown)
		{
			IUnresolvedEntity returnEntity;
			var editor = IdeApp.Workbench.ActiveDocument.Editor;

			var entities = isDirectionDown ? GetEntities () : GetEntities ().Reverse ();
			var enumerator = entities.GetEnumerator ();
			var lineNumber = editor.Caret.Line;

			while (enumerator.MoveNext ()) {
				var currentEntity = enumerator.Current;
				if (isDirectionDown) {
					if (currentEntity.Region.BeginLine > lineNumber) {
						return currentEntity;
					}
				} else {
					if (currentEntity.Region.BeginLine < lineNumber) {
						return currentEntity;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gotos the next entity.
		/// </summary>
		public void GotoNextEntity ()
		{
			var member = GetEntityAtCaret ();
			if (member == null) {
				member = GetNearestEntity (true);
				if (member != null) {
					GotoMember (member);
				}
			} else {
				var members = GetEntities ();
				var index = GetIndexOfMember (member);

				if (index + 1 < members.Count) {

					GotoMember (members [index + 1]);
				}
			}

		}

		/// <summary>
		/// Gotos the previous entity.
		/// </summary>
		public void GotoPreviousEntity ()
		{
			var member = GetEntityAtCaret ();
			var editor = IdeApp.Workbench.ActiveDocument.Editor;
			if (member == null) {
				member = GetNearestEntity (false);
				if (member != null) {
					GotoMember (member);
				}
			} else if (member.Region.BeginLine < editor.Caret.Line) {
				GotoMember (member);
			} else {
				var members = GetEntities ();
				var index = GetIndexOfMember (member);
				if (index - 1 > 0) {
					GotoMember (members [index - 1]);
				}
			}
		}

		#endregion


		#region private impl

	
		/// <summary>
		/// Gets the index of member.
		/// </summary>
		/// <returns>The index of member.</returns>
		/// <param name="member">Member.</param>
		int GetIndexOfMember (IUnresolvedEntity member)
		{
			int i = 0;
			for (i = 0; i < _cachedEntities.Count; i++) {
				if (_cachedEntities [i].Region == member.Region) {
					return i;
				}
			}
			return -1;
		}

		#endregion

	
	}

}

