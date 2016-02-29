using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MonoDevelop.Ide;
using Mono.TextEditor;

[assembly: InternalsVisibleTo ("ComfortAddInTests")]
namespace TwinTechs.EditorExtensions.Helpers
{
	public class XamlHelper
	{
		public XamlHelper ()
		{
		}

		internal static string GetWordAtColumn (string text, int column)
		{
			var pattern = @"\w+";
			var matches = Regex.Matches (text, pattern);
			foreach (Match match in matches) {
				if (match.Index < column && column < (match.Index + match.Length)) {
					return match.Value;
				}
			}
			return null;
		}


	}
}

