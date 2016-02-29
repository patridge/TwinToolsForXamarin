using System;
using System.Linq;
using System.Collections.Generic;

namespace TwinTechs.EditorExtensions.Extensions
{
	public static class IEnumerableExtensions
	{
		public static int IndexOf<TSource> (this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{

			var index = 0;
			foreach (var item in source) {
				if (predicate.Invoke (item)) {
					return index;
				}
				index++;
			}

			return -1;
		}
	}
}

