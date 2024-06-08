using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte
{
	public static class Extensions
	{
		public static IEnumerable<T> DistinctOrderBy<T>(this IEnumerable<T> src, Comparison<T> comp)
		{
			List<T> srcList = src.ToList();
			List<T> dest = new List<T>();

			srcList.Sort(comp);

			if (1 <= srcList.Count)
			{
				dest.Add(srcList[0]);

				for (int index = 1; index < srcList.Count; index++)
					if (comp(srcList[index - 1], srcList[index]) != 0)
						dest.Add(srcList[index]);
			}
			return dest;
		}

		public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> src, Comparison<T> comp)
		{
			List<T> list = src.ToList();
			list.Sort(comp);
			return list;
		}
	}
}
