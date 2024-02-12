using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class KnapsackProblemTools
	{
		// 使用例
#if false
		public void Test01()
		{
			Item_t[] items = SCommon.Generate(SCommon.CRandom.GetRange(0, 20), () => new Item_t()
			{
				Price = SCommon.CRandom.GetRange(0, 1000),
				Weight = SCommon.CRandom.GetRange(0, 1000),
			})
			.ToArray();
			int weightCapacity = SCommon.CRandom.GetRange(0, items.Length * 1000 + 30);

			Item_t[] ans = Test01_a(items, weightCapacity);

			foreach (Item_t item in ans)
			{
				Console.WriteLine(item.Price);
			}
		}

		private class Item_t
		{
			public int Price;
			public int Weight;
		}

		private Item_t[] Test01_a(Item_t[] items, int weightCapacity)
		{
			KnapsackProblemTools.Item<Item_t>[] prm = items.Select(v => new KnapsackProblemTools.Item<Item_t>()
			{
				Value = v,
				Price = v.Price,
				Weight = v.Weight,
			})
			.ToArray();

			KnapsackProblemTools.Item<Item_t>[] ans = KnapsackProblemTools.Search(prm, weightCapacity);

			return ans.Select(v => v.Value).ToArray();
		}
#endif

		/// <summary>
		/// 探索用アイテム
		/// </summary>
		/// <typeparam name="T">アイテムの型</typeparam>
		public class Item<T>
		{
			/// <summary>
			/// アイテム
			/// </summary>
			public T Value;

			/// <summary>
			/// アイテムの価値
			/// 値域：
			/// -- 0 ～ int.MaxValue
			/// </summary>
			public int Price;

			/// <summary>
			/// アイテムの重さ
			/// 値域：
			/// -- 0 ～ int.MaxValue
			/// </summary>
			public int Weight;
		}

		private class SelectedItem
		{
			public int ItemIndex;
			public long TotalPrice;
			public SelectedItem Prev;
		}

		/// <summary>
		/// 探索する。
		/// </summary>
		/// <typeparam name="T">アイテムの型</typeparam>
		/// <param name="items">探索用アイテムの配列</param>
		/// <param name="weightCapacity">ナップサックの容量</param>
		/// <returns>ナップサックに入る最高価値となる探索用アイテムの配列</returns>
		public static Item<T>[] Search<T>(Item<T>[] items, int weightCapacity)
		{
			// 引数チェック
			{
				if (
					items == null ||
					weightCapacity < 0 || SCommon.IMAX < weightCapacity
					)
					throw new Exception("Bad params");

				foreach (Item<T> item in items)
				{
					if (
						item == null ||
						item.Price < 0 ||
						item.Weight < 0
						)
						throw new Exception("Bad item");
				}
			}

			// 空リストに対しては空リストを返す。
			if (items.Length == 0)
				return new Item<T>[0];

			SelectedItem[,] table = new SelectedItem[items.Length, weightCapacity + 1];
			SelectedItem best = null;
			SelectedItem seli;

			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].Weight <= weightCapacity)
				{
					table[i, items[i].Weight] = seli = new SelectedItem()
					{
						ItemIndex = i,
						TotalPrice = (long)items[i].Price,
						Prev = null,
					};

					if (best == null || best.TotalPrice < seli.TotalPrice)
						best = seli;
				}
			}
			for (int i = 0; i < items.Length; i++)
			{
				for (int w = 0; w <= weightCapacity; w++)
				{
					if (table[i, w] != null)
					{
						for (int ni = i + 1; ni < items.Length; ni++)
						{
							if (items[ni].Weight <= weightCapacity - w)
							{
								long nv = table[i, w].TotalPrice + (long)items[ni].Price;
								int nw = w + items[ni].Weight;

								if (table[ni, nw] == null || table[ni, nw].TotalPrice < nv)
								{
									table[ni, nw] = seli = new SelectedItem()
									{
										ItemIndex = ni,
										TotalPrice = nv,
										Prev = table[i, w],
									};

									if (best == null || best.TotalPrice < seli.TotalPrice)
										best = seli;
								}
							}
						}
					}
				}
			}
			List<Item<T>> ret = new List<Item<T>>();

			while (best != null)
			{
				ret.Add(items[best.ItemIndex]);
				best = best.Prev;
			}
			return ret.ToArray();
		}
	}
}
