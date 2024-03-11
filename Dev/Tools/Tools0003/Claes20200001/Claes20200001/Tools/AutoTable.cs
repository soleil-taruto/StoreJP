using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tools
{
	public class AutoTable<T>
	{
		private List<List<T>> Inner = new List<List<T>>();

		public T this[int x, int y]
		{
			get
			{
				if (
					this.Inner.Count <= x ||
					this.Inner[x].Count <= y
					)
					return default(T);

				return this.Inner[x][y];
			}

			set
			{
				while (this.Inner.Count <= x)
					this.Inner.Add(new List<T>());

				while (this.Inner[x].Count <= y)
					this.Inner[x].Add(default(T));

				this.Inner[x][y] = value;
			}
		}

		public int GetLength(int dimension)
		{
			if (dimension == 0)
				return this.Inner.Count;

			if (dimension == 1)
				return this.Inner.Count == 0 ? 0 : this.Inner.Max(column => column.Count);

			throw new Exception("Bad dimension");
		}

		public T[,] ToFixedTable()
		{
			int w = this.GetLength(0);
			int h = this.GetLength(1);

			T[,] dest = new T[w, h];

			for (int x = 0; x < w; x++)
				for (int y = 0; y < h; y++)
					dest[x, y] = this[x, y];

			return dest;
		}
	}
}
