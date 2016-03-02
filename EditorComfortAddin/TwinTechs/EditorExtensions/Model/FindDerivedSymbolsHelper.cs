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
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Model
{
	public class FindDerivedSymbolsHelper : CommandHandler
	{
		static Func<Project, ICompilation> _lookupFunction;


		#region public api


		//
		// Static Methods
		//
		public static void FindDerivedClasses(ITypeDefinition cls, Action<ISearchProgressMonitor, IEntity> reportFunction)
		{
			FindDerivedSymbolsHelper.FindDerivedSymbols(cls, null, reportFunction);
		}



		public static void FindDerivedMembers(IMember member, Action<ISearchProgressMonitor, IEntity> reportFunction)
		{
			ITypeDefinition declaringTypeDefinition = member.DeclaringTypeDefinition;
			if (declaringTypeDefinition != null)
			{
				FindDerivedSymbolsHelper.FindDerivedSymbols(declaringTypeDefinition, member, reportFunction);
			}
		}

		#endregion


		private static IMember FindDerivedMember(IMember importedMember, ITypeDefinition derivedType)
		{
			IMember result;
			if (importedMember.DeclaringTypeDefinition.Kind == TypeKind.Interface)
			{
				result = derivedType.GetMembers(null, GetMemberOptions.IgnoreInheritedMembers).FirstOrDefault((IMember m) => m.ImplementedInterfaceMembers.Any((IMember im) => im.Region == importedMember.Region));
			}
			else {
				result = InheritanceHelper.GetDerivedMember(importedMember, derivedType);
			}
			return result;
		}

		private static void FindDerivedSymbols(ITypeDefinition cls, IMember member, Action<ISearchProgressMonitor, IEntity> reportFunction)
		{
			Solution currentSelectedSolution = IdeApp.ProjectOperations.CurrentSelectedSolution;
			if (currentSelectedSolution != null)
			{
				//FIXME - needs rewriting
				Project project = TypeSystemService.GetMonoProject(cls);
				if (project != null)
				{
					IEnumerable<Project> referencingProjects = ReferenceFinder.GetAllReferencingProjects(currentSelectedSolution, project);
					if (FindDerivedSymbolsHelper._lookupFunction == null)
					{
						FindDerivedSymbolsHelper._lookupFunction = new Func<Project, ICompilation>(TypeSystemService.GetCompilation);
					}
					List<ICompilation> list = (from c in referencingProjects.Select(FindDerivedSymbolsHelper._lookupFunction)
											   where c != null
											   select c).ToList<ICompilation>();
					Parallel.ForEach<ICompilation>(list, delegate (ICompilation comp)
					{
						try
						{
							FindDerivedSymbolsHelper.SearchCompilation(null, comp, cls, member, reportFunction);
						}
						catch (Exception ex)
						{
							LoggingService.LogInternalError(ex);
						}
					});
				}
			}
		}

		private static void SearchCompilation(ISearchProgressMonitor monitor, ICompilation comp, ITypeDefinition cls, IMember member, Action<ISearchProgressMonitor, IEntity> reportFunction)
		{
			ITypeDefinition typeDefinition = TypeSystemExtensions.Import(comp, cls);
			if (typeDefinition != null)
			{
				IMember member2 = null;
				if (member != null)
				{
					member2 = TypeSystemExtensions.Import(comp, member);
					if (member2 == null)
					{
						return;
					}
				}
				foreach (ITypeDefinition current in TypeSystemExtensions.GetAllTypeDefinitions(comp.MainAssembly))
				{
					if (TypeSystemExtensions.IsDerivedFrom(current, typeDefinition))
					{
						IEntity entity;
						if (member != null)
						{
							entity = FindDerivedSymbolsHelper.FindDerivedMember(member2, current);
							if (entity == null)
							{
								continue;
							}
						}
						else {
							entity = current;
						}
						//at this point we can jump to the first one
						reportFunction(monitor, entity);
					}
				}
			}
		}

		//		protected override void Run (object data)
		//		{
		//			Document activeDocument = IdeApp.Workbench.ActiveDocument;
		//			if (activeDocument != null && !(activeDocument.FileName == FilePath.Null)) {
		//				ResolveResult resolveResult;
		//				object item = CurrentRefactoryOperationsHandler.GetItem (activeDocument, out resolveResult);
		//				ITypeDefinition typeDefinition = item as ITypeDefinition;
		//				if (typeDefinition != null && ((typeDefinition.Kind == TypeKind.Class && !typeDefinition.IsSealed) || typeDefinition.Kind == TypeKind.Interface)) {
		//					FindDerivedSymbolsHelper.FindDerivedClasses (typeDefinition);
		//				} else {
		//					IMember member = item as IMember;
		//					var symbolsHandler = new FindDerivedSymbolsHandler (member);
		//					if (symbolsHandler.IsValid) {
		//						symbolsHandler.Run ();
		//					}
		//				}
		//			}
		//		}



	}
}

