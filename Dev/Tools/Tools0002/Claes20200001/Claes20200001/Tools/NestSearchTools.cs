using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class NestSearchTools
	{
		// 使用例
#if false
		/// <summary>
		/// ナップサック問題
		/// </summary>
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
			int[] states = new int[items.Length]; // { 0: 初期値, 1: 入れる, 2: 入れない }
			Item_t[] ans = new Item_t[0];

			NestSearchTools.Search(
				count =>
				{
					int totalWeight = Enumerable.Range(0, count).Where(v => states[v] == 1).Sum(v => items[v].Weight);

					if (weightCapacity < totalWeight)
						return true;

					int totalPrice = Enumerable.Range(0, count).Where(v => states[v] == 1).Sum(v => items[v].Price);

					if (ans.Sum(v => v.Price) < totalPrice)
						ans = Enumerable.Range(0, count).Where(v => states[v] == 1).Select(v => items[v]).ToArray();

					return false;
				},
				count => items.Length <= count,
				count => { },
				index => states[index] = 0,
				index => ++states[index] <= 2
				);

			return ans;
		}

		/// <summary>
		/// 桂馬飛び問題
		/// </summary>
		public void Test02()
		{
			string RES_MAP = @"

*******
*S....*
**...G*
**...**
*******

";

			int w;
			int h;
			char[,] map;

			{
				string[] lines = SCommon.TextToLines(RES_MAP)
					.Select(v => v.Trim())
					.Where(v => v != "")
					.ToArray();

				w = lines[0].Length;
				h = lines.Length;
				map = new char[w, h];

				for (int y = 0; y < h; y++)
				{
					string line = lines[y];

					for (int x = 0; x < w; x++)
					{
						map[x, y] = line[x];
					}
				}
			}

			T02_Point startPt = null;

			for (int x = 0; x < w; x++)
				for (int y = 0; y < h; y++)
					if (map[x, y] == 'S')
						startPt = new T02_Point(x, y);

			T02_Point[] KEIMA_TOBI_LIST = new T02_Point[]
			{
				new T02_Point(-1, -2),
				new T02_Point(-2, -1),
				new T02_Point(-2, 1),
				new T02_Point(-1, 2),
				new T02_Point(1, 2),
				new T02_Point(2, 1),
				new T02_Point(2, -1),
				new T02_Point(1, -2),
			};

			List<T02_Point> route = new List<T02_Point>();
			List<int> moveDirList = new List<int>();

			NestSearchTools.Search(
				count =>
				{
					if (count == 0)
						return false;

					T02_Point currPt = route[count - 1];

					if (
						currPt.X < 0 || w <= currPt.X ||
						currPt.Y < 0 || h <= currPt.Y
						)
						return true;

					if (map[currPt.X, currPt.Y] == '*')
						return true;

					for (int i = 0; i + 1 < count; i++)
					{
						if (
							route[i].X == currPt.X &&
							route[i].Y == currPt.Y
							)
							return true;
					}
					return false;
				},
				count =>
				{
					if (count == 0)
						return false;

					T02_Point currPt = route[count - 1];

					return map[currPt.X, currPt.Y] == 'G';
				},
				count =>
				{
					Console.WriteLine(string.Join(" -> ", route.Take(count).Select(v => v.X + ", " + v.Y)));
				},
				index =>
				{
					while (route.Count <= index)
						route.Add(null);

					while (moveDirList.Count <= index)
						moveDirList.Add(0);

					moveDirList[index] = -1;
				},
				index =>
				{
					moveDirList[index]++;

					if (index == 0)
					{
						if (1 <= moveDirList[index])
							return false;

						route[0] = startPt;
					}
					else
					{
						if (KEIMA_TOBI_LIST.Length <= moveDirList[index])
							return false;

						route[index] = new T02_Point(
							route[index - 1].X + KEIMA_TOBI_LIST[moveDirList[index]].X,
							route[index - 1].Y + KEIMA_TOBI_LIST[moveDirList[index]].Y
							);
					}
					return true;
				}
				);
		}

		private class T02_Point
		{
			public int X;
			public int Y;

			public T02_Point(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

		/// <summary>
		/// 分銅配分問題
		/// ランダムな重さの分銅をそれぞれの重さの合計がなるべく同じになるように2つに分ける。
		/// </summary>
		public void Test03()
		{
			int[] fundouList = SCommon.Generate(SCommon.CRandom.GetRange(0, 20), () => SCommon.CRandom.GetRange(1, 100)).ToArray();
			int[] fundouFuriwakeList = new int[fundouList.Length];
			List<int> fundouList1 = new List<int>();
			List<int> fundouList2 = new List<int>();
			int[] bestFundouList1 = fundouList.ToArray();
			int[] bestFundouList2 = new int[0];

			NestSearchTools.Search(
				count =>
				{
					int weightDiff = Math.Abs(fundouList1.Sum() - fundouList2.Sum());
					int arieruWeightDiff = weightDiff - fundouList.Skip(count).Sum();
					int bestWeightDiff = Math.Abs(bestFundouList1.Sum() - bestFundouList2.Sum());

					return bestWeightDiff <= arieruWeightDiff;
				},
				count => fundouList.Length <= count,
				count =>
				{
					int weightDiff = Math.Abs(fundouList1.Sum() - fundouList2.Sum());
					int bestWeightDiff = Math.Abs(bestFundouList1.Sum() - bestFundouList2.Sum());

					if (weightDiff < bestWeightDiff)
					{
						bestFundouList1 = fundouList1.ToArray();
						bestFundouList2 = fundouList2.ToArray();
					}
				},
				index => fundouFuriwakeList[index] = 0,
				index =>
				{
					fundouFuriwakeList[index]++;
					bool ret = true;

					if (fundouFuriwakeList[index] == 1)
					{
						fundouList1.Add(fundouList[index]);
					}
					else if (fundouFuriwakeList[index] == 2)
					{
						SCommon.UnaddElement(fundouList1);
						fundouList2.Add(fundouList[index]);
					}
					else if (fundouFuriwakeList[index] == 3)
					{
						SCommon.UnaddElement(fundouList2);
						ret = false;
					}
					else
					{
						throw null; // never
					}
					return ret;
				}
				);

			Console.WriteLine(string.Join(" ", bestFundouList1));
			Console.WriteLine(string.Join(" ", bestFundouList2));
		}
#endif

		/// <summary>
		/// 探索する。
		/// </summary>
		/// <param name="isInvalid">0 ～ (引数(count) - 1) の element の並びが間違っていれば ture そうでなければ false</param>
		/// <param name="isEnd">0 ～ (引数(count) - 1) の element の並びで完成していれば true そうでなければ false</param>
		/// <param name="ended">element の並びが完成したことを通知する。引数(count)は element の個数</param>
		/// <param name="resetElement">引数(index)の element を初期化する。次の MoveNextElement によって最初の値に遷移する。</param>
		/// <param name="moveNextElement">引数(index)の element を次の値に遷移する。次の値が無い場合のみ false を返す。</param>
		public static void Search(
			Func<int, bool> isInvalid,
			Func<int, bool> isEnd,
			Action<int> ended,
			Action<int> resetElement,
			Func<int, bool> moveNextElement
			)
		{
			int index = -1;

		forward:
			if (isInvalid(++index))
				goto back;

			if (isEnd(index))
			{
				ended(index);
				goto back;
			}
			resetElement(index);

		next:
			if (moveNextElement(index))
				goto forward;

		back:
			if (0 <= --index)
				goto next;
		}
	}
}
