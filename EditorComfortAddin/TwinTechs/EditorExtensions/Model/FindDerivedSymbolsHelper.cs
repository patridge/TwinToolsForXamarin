using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.TextEditor;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.FindInFiles;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MonoDevelop.Refactoring;

namespace TwinTechs.EditorExtensions.Model
{
	public class FindDerivedSymbolsHelper : CommandHandler
	{
		static Func<Project, ICompilation> _lookupFuncion;

		static MemberReference _currentMemberReference;
		static List<MemberReference> _foundMemberReferences;
		static bool _didFindFirstResult;

		#region public api


		//
		// Static Methods
		//
		public static void FindDerivedClasses (ITypeDefinition cls)
		{
			ResetResults ();
			_didFindFirstResult = false;
			FindDerivedSymbolsHelper.FindDerivedSymbols (cls, null);
		}


		public static void CycleResults ()
		{
			if (_foundMemberReferences.Count > 1) {
				var currentIndex = _foundMemberReferences.IndexOf (_currentMemberReference);
				currentIndex = currentIndex == _foundMemberReferences.Count - 1 ? 0 : currentIndex + 1;
				_currentMemberReference = _foundMemberReferences [currentIndex];
				Gtk.Application.Invoke (delegate {
					IdeApp.Workbench.OpenDocument (_currentMemberReference.FileName, null, _currentMemberReference.Region.BeginLine, _currentMemberReference.Region.BeginColumn, OpenDocumentOptions.Default);
				});
			}
		}


		public static void FindDerivedMembers (IMember member)
		{
			ResetResults ();

			ITypeDefinition declaringTypeDefinition = member.DeclaringTypeDefinition;
			if (declaringTypeDefinition != null) {
				FindDerivedSymbolsHelper.FindDerivedSymbols (declaringTypeDefinition, member);
			}
		}

		#endregion


		private static IMember FindDerivedMember (IMember importedMember, ITypeDefinition derivedType)
		{
			IMember result;
			if (importedMember.DeclaringTypeDefinition.Kind == TypeKind.Interface) {
				result = derivedType.GetMembers (null, GetMemberOptions.IgnoreInheritedMembers).FirstOrDefault ((IMember m) => m.ImplementedInterfaceMembers.Any ((IMember im) => im.Region == importedMember.Region));
			} else {
				result = InheritanceHelper.GetDerivedMember (importedMember, derivedType);
			}
			return result;
		}

		private static void FindDerivedSymbols (ITypeDefinition cls, IMember member)
		{
			Solution currentSelectedSolution = IdeApp.ProjectOperations.CurrentSelectedSolution;
			if (currentSelectedSolution != null) {
				Project project = TypeSystemService.GetProject (cls);
				if (project != null) {
					IEnumerable<Project> referencingProjects = ReferenceFinder.GetAllReferencingProjects (currentSelectedSolution, project);
					if (FindDerivedSymbolsHelper._lookupFuncion == null) {
						FindDerivedSymbolsHelper._lookupFuncion = new Func<Project, ICompilation> (TypeSystemService.GetCompilation);
					}
					List<ICompilation> list = (from c in referencingProjects.Select (FindDerivedSymbolsHelper._lookupFuncion)
					                           where c != null
					                           select c).ToList<ICompilation> ();
					Parallel.ForEach<ICompilation> (list, delegate (ICompilation comp) {
						try {
							FindDerivedSymbolsHelper.SearchCompilation (null, comp, cls, member);
						} catch (Exception ex) {
							LoggingService.LogInternalError (ex);
						}
					});

//					using (ISearchProgressMonitor monitor = IdeApp.Workbench.ProgressMonitors.GetSearchProgressMonitor (true, true)) {
//						string text = (member != null) ? GettextCatalog.GetString ("Searching for implementations...") : GettextCatalog.GetString ("Searching for implementations...");
//						monitor.BeginTask (text, list.Count);
//						Parallel.ForEach<ICompilation> (list, delegate (ICompilation comp) {
//							try {
//								FindDerivedSymbolsHelper.SearchCompilation (monitor, comp, cls, member);
//							} catch (Exception ex) {
//								LoggingService.LogInternalError (ex);
//								monitor.ReportError ("Unhandled error while searching", ex);
//							}
//							monitor.Step (1);
//						});
//						monitor.EndTask ();
//					}
				}
			}
		}

		private static void ReportResult (ISearchProgressMonitor monitor, IEntity result)
		{
			string fileName = result.Region.FileName;
			if (!string.IsNullOrEmpty (fileName)) {
				TextEditorData textEditorData = TextFileProvider.Instance.GetTextEditorData (fileName);
				int num = textEditorData.LocationToOffset (result.Region.Begin);
				textEditorData.SearchRequest.SearchPattern = result.Name;
				Mono.TextEditor.SearchResult searchResult = textEditorData.SearchForward (num);
				if (searchResult != null) {
					num = searchResult.Offset;
				}
				if (textEditorData.Parent == null) {
					textEditorData.Dispose ();
				}

				//only report the results if we have more than one.
				if (_currentMemberReference != null && _didFindFirstResult) {
//					monitor.ReportResult (_currentMemberReference);
				}

				var memberReference = new MemberReference (result, result.Region, num, result.Name.Length);
				if (!_didFindFirstResult) {
					_didFindFirstResult = true;
					_currentMemberReference = memberReference;
					Gtk.Application.Invoke (delegate {
						IdeApp.Workbench.OpenDocument (memberReference.FileName, null, memberReference.Region.BeginLine, memberReference.Region.BeginColumn, OpenDocumentOptions.Default);
					});
				} else {
//					monitor.ReportResult (memberReference);
				}
				_foundMemberReferences.Add (memberReference);
			}
		}

		private static void SearchCompilation (ISearchProgressMonitor monitor, ICompilation comp, ITypeDefinition cls, IMember member)
		{
			ITypeDefinition typeDefinition = TypeSystemExtensions.Import (comp, cls);
			if (typeDefinition != null) {
				IMember member2 = null;
				if (member != null) {
					member2 = TypeSystemExtensions.Import (comp, member);
					if (member2 == null) {
						return;
					}
				}
				foreach (ITypeDefinition current in TypeSystemExtensions.GetAllTypeDefinitions (comp.MainAssembly)) {
					if (TypeSystemExtensions.IsDerivedFrom (current, typeDefinition)) {
						IEntity entity;
						if (member != null) {
							entity = FindDerivedSymbolsHelper.FindDerivedMember (member2, current);
							if (entity == null) {
								continue;
							}
						} else {
							entity = current;
						}
						//at this point we can jump to the first one
						FindDerivedSymbolsHelper.ReportResult (monitor, entity);
					}
				}
			}
		}

		//
		// Methods
		//
		protected override void Run (object data)
		{
			Document activeDocument = IdeApp.Workbench.ActiveDocument;
			if (activeDocument != null && !(activeDocument.FileName == FilePath.Null)) {
				ResolveResult resolveResult;
				object item = CurrentRefactoryOperationsHandler.GetItem (activeDocument, out resolveResult);
				ITypeDefinition typeDefinition = item as ITypeDefinition;
				if (typeDefinition != null && ((typeDefinition.Kind == TypeKind.Class && !typeDefinition.IsSealed) || typeDefinition.Kind == TypeKind.Interface)) {
					FindDerivedSymbolsHelper.FindDerivedClasses (typeDefinition);
				} else {
					IMember member = item as IMember;
					var symbolsHandler = new FindDerivedSymbolsHandler (member);
					if (symbolsHandler.IsValid) {
						symbolsHandler.Run ();
					}
				}
			}
		}


		static void ResetResults ()
		{
			_currentMemberReference = null;
			_foundMemberReferences = new List<MemberReference> ();
			_didFindFirstResult = false;
		}
	}

	internal class FindDerivedSymbolsHandler
	{
		//
		// Fields
		//
		private readonly IMember member;

		//
		// Properties
		//
		public bool IsValid {
			get {
				return IdeApp.ProjectOperations.CurrentSelectedSolution != null && TypeSystemService.GetProject (this.member) != null && (this.member.IsVirtual || this.member.IsAbstract || this.member.DeclaringType.Kind == TypeKind.Interface);
			}
		}

		//
		// Constructors
		//
		public FindDerivedSymbolsHandler (IMember member)
		{
			this.member = member;
		}

		//
		// Methods
		//
		public void Run ()
		{
			FindDerivedClassesHandler.FindDerivedMembers (this.member);
		}
	}
}

