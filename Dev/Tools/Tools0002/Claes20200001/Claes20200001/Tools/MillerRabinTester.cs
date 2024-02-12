using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class MillerRabinTester
	{
		/// <summary>
		/// 100以下の素数
		/// </summary>
		private static readonly int[] PNN = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };

		/// <summary>
		/// スモールセット(2^81.*)のしきい値
		/// </summary>
		private static readonly BigInteger SMALL_SET_81_THRESHOLD = BigInteger.Parse("3317044064679887385961981");

		/// <summary>
		/// スモールセット(2^81.*)
		/// if n LT 3,317,044,064,679,887,385,961,981, it is enough to test a = 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, and 41.
		/// </summary>
		private static readonly int[] SMALL_SET_81 = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41 };

		/// <summary>
		/// デフォルトのテスト回数
		/// </summary>
		private const int DEFAULT_K = 64;

		/// <summary>
		/// ミラーラビン素数判定法によって指定された整数が素数であるか判定する。
		/// 判定する整数が素数であったとき誤判定することはない。
		/// 判定する整数が合成数であったとき、高くとも(4^-k)の確率で素数と判定してしまう。
		/// </summary>
		/// <param name="n">判定する整数</param>
		/// <param name="k">テスト回数</param>
		/// <returns>判定結果</returns>
		public static bool IsProbablePrime(BigInteger n, int k = DEFAULT_K)
		{
			if (k < 1 || SCommon.IMAX < k)
				throw new ArgumentException("Bad k");

			if (n <= 1)
				return false;

			if (n <= 100)
				return PNN.Contains((int)n);

			if (n.IsEven)
				return false;

			BigInteger d = n >> 1;
			int r = 0;

			while (d.IsEven)
			{
				d >>= 1;
				r++;
			}

			if (n < SMALL_SET_81_THRESHOLD)
			{
				foreach (int x in SMALL_SET_81)
					if (!MillerRabinTest(x, d, r, n))
						return false;
			}
			else
			{
				int scale = GetScale(n);

				for (int testcnt = 0; testcnt < k; testcnt++)
				{
					BigInteger x = new BigInteger(SCommon.Join(new byte[][] { SCommon.CRandom.GetBytes(scale + 10), new byte[] { 0x00 } })) % (n - 3) + 2; // 2 ～ (n - 2)

					if (!MillerRabinTest(x, d, r, n))
						return false;
				}
			}
			return true;
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

		private static bool MillerRabinTest(BigInteger x, BigInteger d, int r, BigInteger n)
		{
			x = BigInteger.ModPow(x, d, n);

			if (x != 1 && x != n - 1)
			{
				for (int c = r; ; c--)
				{
					if (c <= 0)
						return false;

					x = BigInteger.ModPow(x, 2, n);

					if (x == n - 1)
						break;
				}
			}
			return true;
		}

		private static int GetScale(BigInteger n)
		{
			byte[] bytes = n.ToByteArray();
			int size = bytes.Length;

			while (1 <= size && bytes[size - 1] == 0)
				size--;

			return size;
		}
	}
}
