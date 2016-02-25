using ICSharpCode.NRefactory.TypeSystem;
using MonoDevelop.Core;
using MonoDevelop.Ide.TypeSystem;
using MonoDevelop.Projects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonoDevelop.Ide.FindInFiles;

namespace TwinTechs.EditorExtensions.Model
{
	
	public class SearchCollector
	{
		//
		// Fields
		//
		private ISet<Project> searchedProjects = new HashSet<Project> ();

		private ISet<string> searchedAssemblies = new HashSet<string> ();

		private ISet<Project> collectedProjects = new HashSet<Project> ();

		private IDictionary<Project, ISet<string>> collectedFiles = new Dictionary<Project, ISet<string>> ();

		private bool projectOnly;

		private IEnumerable<object> entities;

		private Solution solution;

		private bool searchProjectAdded;

		private Project searchProject;

		//
		// Constructors
		//
		private SearchCollector (Solution solution, Project searchProject, IEnumerable<object> entities)
		{
			this.solution = solution;
			this.searchProject = searchProject;
			this.entities = entities;
		}

		//
		// Static Methods
		//
		public static SearchCollector.FileList CollectDeclaringFiles (IEntity entity)
		{
			SearchCollector.FileList result;
			if (entity is ITypeDefinition) {
				result = SearchCollector.CollectDeclaringFiles (entity, from p in (entity as ITypeDefinition).Parts
				                                                        select p.Region.FileName);
			} else if (entity is IMethod) {
				result = SearchCollector.CollectDeclaringFiles (entity, from p in (entity as IMethod).Parts
				                                                        select p.Region.FileName);
			} else {
				result = SearchCollector.CollectDeclaringFiles (entity, new string[] {
					entity.Region.FileName
				});
			}
			return result;
		}

		private static SearchCollector.FileList CollectDeclaringFiles (IEntity entity, IEnumerable<string> fileNames)
		{
			Project project = TypeSystemService.GetProject (entity);
			IEnumerable<FilePath> files = from p in fileNames.Distinct<string> ()
			                              select new FilePath (p);
			return new SearchCollector.FileList (project, TypeSystemService.GetProjectContext (project), files);
		}

		public static IEnumerable<SearchCollector.FileList> CollectFiles (Solution solution, IEnumerable<object> entities)
		{
			return new SearchCollector (solution, null, entities).CollectFiles ();
		}

		public static IEnumerable<SearchCollector.FileList> CollectFiles (Project project, IEnumerable<object> entities)
		{
			return new SearchCollector (project.ParentSolution, project, entities).CollectFiles ();
		}

		public static IEnumerable<Project> CollectProjects (Solution solution, IEnumerable<object> entities)
		{
			return new SearchCollector (solution, null, entities).CollectProjects ();
		}

		private static IEnumerable<Project> GetAllReferencingProjects (Solution solution, string assemblyName)
		{
			return from project in solution.GetAllProjects ()
			       where TypeSystemService.GetCompilation (project).Assemblies.Any ((IAssembly a) => a.AssemblyName == assemblyName)
			       select project;
		}

		//
		// Methods
		//
		private void AddFiles (Project project, IEnumerable<string> files)
		{
			if (project == null) {
				throw new ArgumentNullException ("project");
			}
			if (!this.collectedProjects.Contains (project)) {
				ISet<string> set;
				if (!this.collectedFiles.TryGetValue (project, out set)) {
					set = new HashSet<string> ();
					this.collectedFiles [project] = set;
				}
				foreach (string current in files) {
					set.Add (current);
				}
			}
		}

		private void AddProject (Project project)
		{
			if (project == null) {
				throw new ArgumentNullException ("project");
			}
			this.searchProjectAdded = (project == this.searchProject);
			if (this.collectedProjects.Add (project)) {
				this.collectedFiles.Remove (project);
			}
		}

		private void Collect (Project sourceProject, IEntity entity, bool searchInProject = false)
		{
			if (!this.searchedProjects.Contains (sourceProject)) {
				if (this.searchProject != null && sourceProject != this.searchProject) {
					this.AddProject (this.searchProject);
				} else if (sourceProject == null) {
					if (entity == null) {
						foreach (Project current in this.solution.GetAllProjects ()) {
							this.AddProject (current);
						}
					} else {
						string assemblyName = entity.ParentAssembly.AssemblyName;
						if (this.searchedAssemblies.Add (assemblyName)) {
							foreach (Project current2 in SearchCollector.GetAllReferencingProjects (this.solution, assemblyName)) {
								this.AddProject (current2);
							}
						}
					}
				} else if (entity == null) {
					this.AddProject (sourceProject);
				} else {
					ITypeDefinition declaringTypeDefinition = entity.DeclaringTypeDefinition;
					switch (entity.Accessibility) {
					case Accessibility.Public:
					case Accessibility.Protected:
					case Accessibility.Internal:
					case Accessibility.ProtectedOrInternal:
					case Accessibility.ProtectedAndInternal:
						if (declaringTypeDefinition != null) {
							this.Collect (sourceProject, entity.DeclaringTypeDefinition, searchInProject);
						} else if (this.searchProject != null || searchInProject) {
							this.AddProject (sourceProject);
						} else {
							foreach (Project current3 in ReferenceFinder.GetAllReferencingProjects (this.solution, sourceProject)) {
								if (entity.Accessibility == Accessibility.Internal || entity.Accessibility == Accessibility.ProtectedAndInternal) {
									TypeSystemService.ProjectContentWrapper projectContentWrapper = TypeSystemService.GetProjectContentWrapper (current3);
									if (projectContentWrapper == null) {
										continue;
									}
									if (!entity.ParentAssembly.InternalsVisibleTo (projectContentWrapper.Compilation.MainAssembly)) {
										continue;
									}
								}
								this.AddProject (current3);
							}
						}
						break;
					default:
						if (this.projectOnly) {
							this.AddProject (sourceProject);
						} else if (declaringTypeDefinition != null) {
							this.AddFiles (sourceProject, from p in declaringTypeDefinition.Parts
							                              select p.Region.FileName);
						}
						break;
					}
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable<SearchCollector.FileList> CollectFiles ()
		{
			var projects = this.CollectProjects ();
			foreach (var project in projects) {
				foreach (var item in entities) {
					var entity = item as IEntity;
					this.Collect (project, entity, true);
				}
			}
			return null;
		}

		private IEnumerable<Project> CollectProjects ()
		{
			this.projectOnly = true;
			foreach (object current in this.entities) {
				IEntity entity = current as IEntity;
				if (entity != null) {
					this.Collect (TypeSystemService.GetProject (entity), entity, false);
				} else {
					IParameter parameter = current as IParameter;
					if (parameter != null) {
						this.Collect (TypeSystemService.GetProject (parameter.Owner), parameter.Owner, false);
					}
				}
			}
			return this.collectedProjects;
		}

		//
		// Nested Types
		//
		public class FileList
		{
			public Project Project {
				get;
				private set;
			}

			public IProjectContent Content {
				get;
				private set;
			}

			public IEnumerable<FilePath> Files {
				get;
				private set;
			}

			public FileList (Project project, IProjectContent content, IEnumerable<FilePath> files)
			{
				this.Project = project;
				this.Content = content;
				this.Files = files;
			}
		}
	}
}