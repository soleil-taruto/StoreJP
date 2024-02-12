using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tools
{
	public static class PrimeTester
	{
		/// <summary>
		/// 100以下の素数
		/// </summary>
		private static readonly int[] PNN = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };

		/// <summary>
		/// small set
		/// if n LT 4,759,123,141, it is enough to test a = 2, 7, and 61;
		/// </summary>
		private static readonly int[] SMALL_SET_32 = new int[] { 2, 7, 61 };

		/// <summary>
		/// small set
		/// if n LT 18,446,744,073,709,551,616 = 26^4, it is enough to test a = 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, and 37.
		/// </summary>
		private static readonly int[] SMALL_SET_64 = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };

		/// <summary>
		/// ミラーラビン素数判定法によって 0 以上 2^64 未満の整数が素数であるか判定する。
		/// 入力値の大きさに対応するスモールセット(これだけ確認すれば良いという集合)をテストしているので、
		/// このメソッドによる判定結果は常に正しい。
		/// </summary>
		/// <param name="n">判定する整数</param>
		/// <returns>判定結果</returns>
		public static bool IsPrime(ulong n)
		{
			if (n <= 100)
				return PNN.Contains((int)n);

			if (n % 2 == 0)
				return false;

			ulong d = n;
			int r;
			for (r = 0; ((d >>= 1) & 1) == 0; r++) ;

			if (n <= uint.MaxValue)
				return SMALL_SET_32.All(x => MillerRabinTest32((uint)x, (uint)d, r, (uint)n));
			else
				return SMALL_SET_64.All(x => MillerRabinTest64((ulong)x, d, r, n));
		}

#if false // memo:

https://zh.wikipedia.org/wiki/%E7%B1%B3%E5%8B%92-%E6%8B%89%E5%AE%BE%E6%A3%80%E9%AA%8C#%E7%AE%97%E6%B3%95%E5%A4%8D%E6%9D%82%E5%BA%A6

Input #1: n > 3, an odd integer to be tested for primality;
Input #2: k, a parameter that determines the accuracy of the test
Output: composite if n is composite, otherwise probably prime

write n - 1 as (2^r)*d with d odd by factoring powers of 2 from n - 1
WitnessLoop: repeat k times:
	pick a random integer a in the range [2, n - 2]
	x <- a^d mod n
	if x = 1 or x = n - 1 then
		continue WitnessLoop
	repeat r - 1 times:
		x <- x^2 mod n
		if x = n - 1 then
			continue WitnessLoop
	return composite
return probably prime

#endif

		private static bool MillerRabinTest32(uint x, uint d, int r, uint n)
		{
			x = ModPow32(x, d, n);

			if (x != 1 && x != n - 1)
			{
				for (int s = r; ; s--)
				{
					if (s <= 0)
						return false;

					x = (uint)(((ulong)x * x) % n);

					if (x == n - 1)
						break;
				}
			}
			return true;
		}

		private static uint ModPow32(uint b, uint e, uint m)
		{
			uint a = 1;

			for (; e != 0; e >>= 1)
			{
				if ((e & 1) != 0)
					a = (uint)(((ulong)a * b) % m);

				b = (uint)(((ulong)b * b) % m);
			}
			return a;
		}

		private static bool MillerRabinTest64(ulong x, ulong d, int r, ulong n)
		{
			x = ModPow64(x, d, n);

			if (x != 1 && x != n - 1)
			{
				for (int c = r; ; c--)
				{
					if (c <= 0)
						return false;

					x = ModPow64(x, 2, n);

					if (x == n - 1)
						break;
				}
			}
			return true;
		}

		private static ulong ModPow64(ulong b, ulong e, ulong m)
		{
			ulong a = 1;

			for (; e != 0; e >>= 1)
			{
				if ((e & 1) != 0)
					a = ModMul64(a, b, m);

				b = ModMul64(b, b, m);
			}
			return a;
		}

		private static ulong ModMul64(ulong b, ulong e, ulong m)
		{
			ulong a = 0;

			for (; e != 0; e >>= 1)
			{
				if ((e & 1) != 0)
					a = ModAdd64(a, b, m);

				b = ModAdd64(b, b, m);
			}
			return a;
		}

		private static ulong ModAdd64(ulong a, ulong b, ulong m)
		{
			ulong r = (ulong.MaxValue % m + 1) % m;

			while (ulong.MaxValue - a < b)
			{
				unchecked { a += b; }
				b = r;
			}
			return (a + b) % m;
		}
	}
}
