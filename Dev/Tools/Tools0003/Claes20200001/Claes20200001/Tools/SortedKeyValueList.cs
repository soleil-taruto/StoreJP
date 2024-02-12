using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tools
{
	public class SortedKeyValueList<K, V>
	{
		private class KeyValuePair
		{
			public K Key;
			public V Value;
		}

		private SortedList<KeyValuePair> Inner;

		public SortedKeyValueList(Comparison<K> comp)
		{
			this.Inner = new SortedList<KeyValuePair>((a, b) => comp(a.Key, b.Key));
		}

		public void Add(K key, V value)
		{
			this.Inner.Add(new KeyValuePair()
			{
				Key = key,
				Value = value,
			});
		}

		public void RemoveKey(K key)
		{
			this.Inner.RemoveAt(this.GetIndex(key));
		}

		public bool ContainsKey(K key)
		{
			return this.Inner.GetIndex(new KeyValuePair() { Key = key }) != -1;
		}

		public int Count
		{
			get
			{
				return this.Inner.Count;
			}
		}

		public K[] Keys
		{
			get
			{
				return this.Inner.Elements.Select(element => element.Key).ToArray();
			}
		}

		public V this[K key]
		{
			get
			{
				return this.Inner[this.GetIndex(key)].Value;
			}

			set
			{
				this.Inner[this.GetIndex(key)].Value = value;
			}
		}

		private int GetIndex(K key)
		{
			int index = this.Inner.GetIndex(new KeyValuePair() { Key = key });

			if (index == -1)
				throw new Exception("Bad key");

			return index;
		}
	}
}
