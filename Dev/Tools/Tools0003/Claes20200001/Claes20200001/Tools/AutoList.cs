using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tools
{
	public class AutoList<T>
	{
		private List<T> Inner = new List<T>();

		public T this[int index]
		{
			get
			{
				if (this.Inner.Count <= index)
					return default(T);

				return this.Inner[index];
			}

			set
			{
				while (this.Inner.Count <= index)
					this.Inner.Add(default(T));

				this.Inner[index] = value;
			}
		}

		public int Count
		{
			get
			{
				return this.Inner.Count;
			}
		}

		public T[] ToArray()
		{
			return this.Inner.ToArray();
		}
	}
}
