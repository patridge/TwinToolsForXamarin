using System;
using System.Diagnostics;
using MonoDevelop.Ide;
using Atk;
using MonoDevelop.Ide.Gui;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoDevelop.Projects;

namespace TwinTechs.EditorExtensions.Helpers
{
	public class FileHistoryHelper
	{
		static FileHistoryHelper _instance;
		Collection<FileOpenInformation> _recentDocuments;
		const int MaxDocuments = 100;
		MonoDevelop.Ide.Gui.Document _previousDocument;
		Solution _loadedSolution;

		const string PathDelimeter = "#:#";

		static FileHistoryHelper()
		{

			Console.WriteLine("creating file history helper " + Instance);
		}


		public static FileHistoryHelper Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new FileHistoryHelper();
				}
				return _instance;
			}
		}

		public FileHistoryHelper()
		{
			IdeApp.Workbench.ActiveDocumentChanged += IdeApp_Workbench_ActiveDocumentChanged;
			IdeApp.Workspace.SolutionLoaded += IdeApp_Workspace_SolutionChanged;
			IdeApp.Workspace.SolutionUnloaded += IdeApp_Workspace_SolutionChanged;
			IdeApp.Exiting += IdeApp_Exiting;
		}

		void IdeApp_Workspace_SolutionChanged(object sender, SolutionEventArgs e)
		{
			SaveHistory();
		}


		void IdeApp_Exiting(object sender, ExitEventArgs args)
		{
			SaveHistory();
		}

		#region events

		void IdeApp_Workbench_ActiveDocumentChanged(object sender, EventArgs e)
		{
			//TODO track this better in roslyn
			if (_previousDocument != null && _previousDocument.Editor != null)
			{
				UpdateFileOpenInfo(_previousDocument, _previousDocument.Editor.CaretLine, _previousDocument.Editor.CaretColumn);
			}

			if (IdeApp.Workbench != null && IdeApp.Workbench.ActiveDocument != null)
			{
				var document = IdeApp.Workbench.ActiveDocument;
				if (document != null && document.FileName != null)
				{

					var existingFileInfo = _recentDocuments.FirstOrDefault((arg) => arg.Project.ItemId == document.Project.ItemId && arg.FileName.FullPath == document.FileName.FullPath);
					var lineNumber = existingFileInfo?.Line ?? 1;
					var column = existingFileInfo?.Column ?? 1;
					UpdateFileOpenInfo(document, lineNumber, column);
				}
			}
			_previousDocument = IdeApp.Workbench.ActiveDocument;

			//TODO experimental - probably want to move this to ide save, or every 30 seconds or so..
			SaveHistory();
		}

		void UpdateFileOpenInfo(MonoDevelop.Ide.Gui.Document document, int line, int column)
		{
			try
			{

				var existingFileInfo = _recentDocuments.FirstOrDefault((arg) => arg.Project.ItemId == document.Project.ItemId && arg.FileName.FullPath == document.FileName.FullPath);
				if (existingFileInfo != null)
				{
					_recentDocuments.Remove(existingFileInfo);
				}
				if (GetProjectWithId(document.Project.ItemId) != null && File.Exists(document.FileName.FullPath))
				{
					var fileInfo = new FileOpenInformation(document.FileName.FullPath, document.Project, line, column, OpenDocumentOptions.BringToFront | OpenDocumentOptions.TryToReuseViewer);
					_recentDocuments.Insert(0, fileInfo);
				}
				if (_recentDocuments.Count >= MaxDocuments)
				{
					var numberOfDocumentsToPurge = _recentDocuments.Count - MaxDocuments;
					for (int i = 0; i < numberOfDocumentsToPurge; i++)
					{
						_recentDocuments.RemoveAt(_recentDocuments.Count - 1);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("error updating file info " + ex.Message);
			}

		}


		#endregion

		#region public api

		/// <summary>
		/// Gets the recent documents.
		/// </summary>
		/// <returns>The recent documents.</returns>
		public Collection<FileOpenInformation> GetRecentDocuments()
		{
			if (_loadedSolution == null || _loadedSolution != IdeApp.ProjectOperations.CurrentSelectedSolution)
			{
				_loadedSolution = IdeApp.ProjectOperations.CurrentSelectedSolution;
				LoadHistory();
			}
			return new Collection<FileOpenInformation>(_recentDocuments);
		}

		#endregion

		#region private impl

		/// <summary>
		/// Loads the history.
		/// </summary>
		void LoadHistory()
		{
			var historyItems = new Collection<FileOpenInformation>();
			try
			{
				var paths = File.ReadAllLines(GetSavedStatePath());
				var splitItems = paths.Select((arg) => arg.Split(new string[] { PathDelimeter }, StringSplitOptions.None));
				foreach (var item in splitItems)
				{
					var project = GetProjectWithId(item[1]);
					var line = int.Parse(item[2]);
					var column = int.Parse(item[3]);
					if (project != null && File.Exists(item[0]))
					{
						var fileOpenInformation = new FileOpenInformation(item[0], project, line, column, OpenDocumentOptions.BringToFront | OpenDocumentOptions.TryToReuseViewer);
						var isDupliate = historyItems.FirstOrDefault(info => info.Project.ItemId == project.ItemId &&
										info.FileName == item[0]) != null;
						if (!isDupliate && historyItems.Count < MaxDocuments)
						{
							historyItems.Add(fileOpenInformation);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("error loading history " + e.Message);
			}
			_recentDocuments = historyItems;
		}


		/// <summary>
		/// Saves the history.
		/// </summary>
		void SaveHistory()
		{
			try
			{
				var newItems = _recentDocuments.Select((e) => new string[] {
					e.FileName.FullPath,
					e.Project.ItemId,
					e.Column.ToString (),
					e.Line.ToString ()
				}).Where(data => File.Exists(data[0]));
				var concatanated = newItems.Select((string[] arg) => string.Join(PathDelimeter, arg));
				File.WriteAllLines(GetSavedStatePath(), concatanated);
			}
			catch (Exception e)
			{
				Console.WriteLine("error saving history " + e.Message);
			}
		}

		/// <summary>
		/// Gets the project with identifier.
		/// </summary>
		/// <returns>The project with identifier.</returns>
		/// <param name="itemId">Item identifier.</param>
		Project GetProjectWithId(string itemId)
		{
			return IdeApp.Workspace.GetAllProjects().FirstOrDefault((project) => project.ItemId == itemId);
		}

		/// <summary>
		/// Saveds the state path.
		/// </summary>
		/// <returns>The state path.</returns>
		string GetSavedStatePath()
		{
			//TODO get a workspace specific base directory
			if (_loadedSolution != null)
			{
				var tempPath = _loadedSolution.BaseDirectory.Combine(".#FileHistory");
				return tempPath;
			}
			else {
				return null;
			}

		}

		#endregion
	}
}

