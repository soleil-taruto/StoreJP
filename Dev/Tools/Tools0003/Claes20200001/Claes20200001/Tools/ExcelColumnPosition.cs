using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Charlotte.Tools
{
	/// <summary>
	/// エクセルの列位置
	/// 列インデックスは１から始まる。
	/// 列インデックスと列名の対応：
	/// -- 1 == A
	/// -- 2 == B
	/// -- 3 == C
	/// -- ･･･
	/// -- 26 == Z
	/// -- 27 == AA
	/// -- 28 == AB
	/// -- 29 == AC
	/// -- ･･･
	/// -- 52 == AZ
	/// -- 53 == BA
	/// -- 54 == BB
	/// -- 55 == BC
	/// -- ･･･
	/// -- 702 == ZZ
	/// -- 703 == AAA
	/// -- 704 == AAB
	/// -- 705 == AAC
	/// -- ･･･
	/// -- 18278 == ZZZ
	/// -- 18279 == AAAA
	/// </summary>
	public struct ExcelColumnPosition
	{
		/// <summary>
		/// 列インデックスを保持する。
		/// 列インデックスは１から始まる。
		/// 例：
		/// -- 1 == A
		/// -- 2 == B
		/// -- 3 == C
		/// </summary>
		private int Index;

		/// <summary>
		/// 列インデックスからインスタンスを作成する。
		/// </summary>
		/// <param name="index">列インデックス</param>
		public ExcelColumnPosition(int index)
		{
			this.Index = index;
		}

		/// <summary>
		/// 列名からインスタンスを作成する。
		/// </summary>
		/// <param name="name">列名</param>
		public ExcelColumnPosition(string name)
		{
			this.Index = ToIndex(name);
		}

		/// <summary>
		/// 列インデックスを取得する。
		/// </summary>
		/// <returns>列インデックス</returns>
		public int GetIndex()
		{
			return this.Index;
		}

		/// <summary>
		/// 列名を取得する。
		/// </summary>
		/// <returns>列名</returns>
		public string GetName()
		{
			return ToName(this.Index);
		}

		/// <summary>
		/// 列名を取得する。
		/// </summary>
		/// <returns>列目</returns>
		public override string ToString()
		{
			return this.GetName();
		}

		/// <summary>
		/// 列インデックスを加算して、移動先の列を取得する。
		/// </summary>
		/// <param name="instance">列</param>
		/// <param name="value">加算する数</param>
		/// <returns>移動先の列</returns>
		public static ExcelColumnPosition operator +(ExcelColumnPosition instance, int value)
		{
			return new ExcelColumnPosition(instance.GetIndex() + value);
		}

		/// <summary>
		/// 列インデックスを減算して、移動先の列を取得する。
		/// </summary>
		/// <param name="instance">列</param>
		/// <param name="value">減算する数</param>
		/// <returns>移動先の列</returns>
		public static ExcelColumnPosition operator -(ExcelColumnPosition instance, int value)
		{
			return new ExcelColumnPosition(instance.GetIndex() - value);
		}

		/// <summary>
		/// 列名を列インデックスに変換する。
		/// 例：
		/// -- A ==> 1
		/// -- B ==> 2
		/// -- C ==> 3
		/// </summary>
		/// <param name="name">列名</param>
		/// <returns>列インデックス</returns>
		public static int ToIndex(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new Exception("no name");

			name = name.ToUpper();

			if (!Regex.IsMatch(name, "^[A-Z]+$"))
				throw new Exception("Bad name");

			int index = 0;

			foreach (char chr in name)
			{
				index *= 26;
				index += (int)(chr - 'A') + 1;
			}
			return index;
		}

		/// <summary>
		/// 列インデックスを列名に変換する。
		/// 例：
		/// -- 1 ==> A
		/// -- 2 ==> B
		/// -- 3 ==> C
		/// </summary>
		/// <param name="index">列インデックス</param>
		/// <returns>列名</returns>
		public static string ToName(int index)
		{
			if (index < 1)
				throw new Exception("Bad index");

			string name = "";

			while (1 <= index)
			{
				index--;
				name = (char)('A' + (index % 26)) + name;
				index /= 26;
			}
			return name;
		}
	}
}
