using System;

namespace TwinTechs.EditorExtensions.Extensions
{
	public static class StringExtensions
	{
		public static string Reverse (string s)
		{
			char[] charArray = s.ToCharArray ();
			Array.Reverse (charArray);
			return new string (charArray);
		}

		public static string ReplaceLastOccurrence (this string Source, string Find, string Replace)
		{
			int place = Source.LastIndexOf (Find);

			if (place == -1)
				return Source;

			string result = Source.Remove (place, Find.Length).Insert (place, Replace);
			return result;
		}
	}
}

