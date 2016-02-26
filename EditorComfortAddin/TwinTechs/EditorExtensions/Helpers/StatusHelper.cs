using System;
using MonoDevelop.Ide;
using MonoDevelop.Core;

namespace TwinTechs.EditorExtensions.Helpers
{
	public static class StatusHelper
	{
		static System.Timers.Timer _timer;

		//TODO - add handlers to track when a document window is closed, and reset the status


		/// <summary>
		/// Shows the status.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="autoHide">If set to <c>true</c> auto hide.</param>
		public static void ShowStatus (string message, bool autoHide = true)
		{
			ShowStatus (default(IconId), message, autoHide);
		}

		/// <summary>
		/// Shows the status, with an icon
		/// </summary>
		/// <param name="iconId">Icon identifier.</param>
		/// <param name="message">Message.</param>
		/// <param name="autoHide">If set to <c>true</c> auto hide.</param>
		public static void ShowStatus (IconId iconId, string message, bool autoHide = true)
		{
			Gtk.Application.Invoke (delegate {
				if (iconId != default(IconId)) {
					IdeApp.Workbench.StatusBar.ShowMessage (iconId, message);
				} else {
					IdeApp.Workbench.StatusBar.ShowMessage (message);
				}
				if (autoHide) {
					StatusHelper.ClearStatusAfterDelay ();
				}
			});
		}

		#region private impl

		static void ClearStatusAfterDelay ()
		{
			if (_timer != null) {
				_timer.Elapsed -= _timer_Elapsed;
				_timer.Stop ();
			}
			_timer = new System.Timers.Timer ();
			_timer.Elapsed += _timer_Elapsed;
			_timer.Interval = 10000;
			_timer.AutoReset = false;
			_timer.Start ();
		}

		static void _timer_Elapsed (object sender, System.Timers.ElapsedEventArgs e)
		{
			Gtk.Application.Invoke (delegate {
				IdeApp.Workbench.StatusBar.ShowMessage ("");
			});
			_timer = null;
		}

		#endregion

	}
}

