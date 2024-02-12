using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public class SortedList<T>
	{
		private List<T> Inner = new List<T>();
		private Comparison<T> Comp;
		private bool SortedFlag = true;

		public SortedList(Comparison<T> comp)
		{
			this.Comp = comp;
		}

		public SortedList(IEnumerable<T> list, Comparison<T> comp)
		{
			foreach (T element in list)
				this.Inner.Add(element);

			this.Comp = comp;
			this.SortedFlag = false;
		}

		public void Add(T element)
		{
			this.Inner.Add(element);
			this.SortedFlag = false;
		}

		public void RemoveAt(int index)
		{
			this.SortIfNeeded();
			this.Inner.RemoveAt(index);
		}

		public int GetIndex(T targetValue)
		{
			this.SortIfNeeded();
			return SCommon.GetIndex(this.Inner, targetValue, this.Comp);
		}

		public int Count
		{
			get
			{
				return this.Inner.Count;
			}
		}

		public T[] Elements
		{
			get
			{
				this.SortIfNeeded();
				return this.Inner.ToArray();
			}
		}

		public T this[int index]
		{
			get
			{
				this.SortIfNeeded();
				return this.Inner[index];
			}

			set
			{
				this.SortIfNeeded();
				this.Inner[index] = value;
			}
		}

		private void SortIfNeeded()
		{
			if (!this.SortedFlag)
			{
				this.Inner.Sort(this.Comp);
				this.SortedFlag = true;
			}
		}
	}
}
