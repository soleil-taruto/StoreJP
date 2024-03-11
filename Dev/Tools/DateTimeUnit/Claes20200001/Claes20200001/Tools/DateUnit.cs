using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	/// <summary>
	/// 日付
	/// 範囲：
	/// -- YEAR_MIN/1/1 ～ YEAR_MAX/12/31
	/// </summary>
	public struct DateUnit
	{
		/// <summary>
		/// このアプリケーションで使用可能な最小の年
		/// </summary>
		public static readonly int YEAR_MIN = 1900;

		/// <summary>
		/// このアプリケーションで使用可能な最大の年
		/// </summary>
		public static readonly int YEAR_MAX = 2400;

		/// <summary>
		/// 最小の日付
		/// </summary>
		public static readonly DateUnit DATE_MIN = new DateUnit(YEAR_MIN, 1, 1);

		/// <summary>
		/// 最大の日付
		/// </summary>
		public static readonly DateUnit DATE_MAX = new DateUnit(YEAR_MAX, 12, 31);

		/// <summary>
		/// 保持している日付
		/// </summary>
		private SimpleDateTime Inner;

		/// <summary>
		/// 日付からインスタンスを作成する。
		/// </summary>
		/// <param name="y">年</param>
		/// <param name="m">月</param>
		/// <param name="d">日</param>
		public DateUnit(int y, int m, int d)
		{
			y = SCommon.ToRange(y, YEAR_MIN, YEAR_MAX);
			m = SCommon.ToRange(m, 1, 12);
			d = SCommon.ToRange(d, 1, DateTime.DaysInMonth(y, m));

			this.Inner = SimpleDateTime.FromTimeStamp(
				y * 10000000000L +
				m * 100000000L +
				d * 1000000L
				);
		}

		/// <summary>
		/// 日付からインスタンスを作成する。
		/// 日付の形式：
		/// -- YYYYMMDD
		/// </summary>
		/// <param name="date">日付</param>
		/// <returns>日付のインスタンス</returns>
		public static DateUnit CreateByValue(int date)
		{
			int y = date / 10000;
			int m = (date / 100) % 100;
			int d = date % 100;

			return new DateUnit(y, m, d);
		}

		/// <summary>
		/// 今日の日付を取得する。
		/// </summary>
		/// <returns>今日の日付</returns>
		public static DateUnit GetToday()
		{
			return CreateByValue((int)(SimpleDateTime.Now().ToTimeStamp() / 1000000));
		}

		/// <summary>
		/// 年
		/// 範囲：
		/// -- YEAR_MIN ～ YEAR_MAX
		/// </summary>
		public int Year
		{
			get
			{
				return this.Inner.Year;
			}
		}

		/// <summary>
		/// 月
		/// 範囲：
		/// -- 1 ～ 12
		/// </summary>
		public int Month
		{
			get
			{
				return this.Inner.Month;
			}
		}

		/// <summary>
		/// 日
		/// 範囲：
		/// -- 1 ～ 31
		/// </summary>
		public int Day
		{
			get
			{
				return this.Inner.Day;
			}
		}

		/// <summary>
		/// 曜日
		/// </summary>
		public char DayOfWeek
		{
			get
			{
				return this.Inner.DayOfWeek;
			}
		}

		/// <summary>
		/// 日付の値を取得する。
		/// 形式：
		/// -- YYYYMMDD
		/// </summary>
		public int GetValue()
		{
			return (int)(this.Inner.ToTimeStamp() / 1000000);
		}

		/// <summary>
		/// 時刻を付与して日時に変換する。
		/// </summary>
		/// <param name="h">時</param>
		/// <param name="i">分</param>
		/// <param name="s">秒</param>
		/// <returns>日時</returns>
		public DateTimeUnit WithTime(int h, int i, int s)
		{
			return new DateTimeUnit(this.Year, this.Month, this.Day, h, i, s);
		}

		/// <summary>
		/// 指定年月の日数を取得する。
		/// </summary>
		/// <param name="y">年</param>
		/// <param name="m">月</param>
		/// <returns>日数</returns>
		public static int GetDaysOfMonth(int y, int m)
		{
			y = SCommon.ToRange(y, YEAR_MIN, YEAR_MAX);
			m = SCommon.ToRange(m, 1, 12);

			return SCommon.TimeStampToSecHelper.GetDaysOfMonth(y, m);
		}

		public static DateUnit operator ++(DateUnit instance)
		{
			return instance + 1;
		}

		public static DateUnit operator --(DateUnit instance)
		{
			return instance - 1;
		}

		public static DateUnit operator +(DateUnit instance, int day)
		{
			return CreateByValue((int)((instance.Inner + (long)day * 86400).ToTimeStamp() / 1000000));
		}

		public static DateUnit operator +(int day, DateUnit instance)
		{
			return CreateByValue((int)((instance.Inner + (long)day * 86400).ToTimeStamp() / 1000000));
		}

		public static DateUnit operator -(DateUnit instance, int day)
		{
			return CreateByValue((int)((instance.Inner - (long)day * 86400).ToTimeStamp() / 1000000));
		}

		public static int operator -(DateUnit a, DateUnit b)
		{
			return (int)((a.Inner - b.Inner) / 86400);
		}

		public static bool operator ==(DateUnit a, DateUnit b)
		{
			return a.Inner == b.Inner;
		}

		public static bool operator !=(DateUnit a, DateUnit b)
		{
			return a.Inner != b.Inner;
		}

		public override bool Equals(object another)
		{
			return another is DateUnit && this == (DateUnit)another;
		}

		public override int GetHashCode()
		{
			return this.Inner.GetHashCode();
		}

		public static bool operator <(DateUnit a, DateUnit b)
		{
			return a.Inner < b.Inner;
		}

		public static bool operator >(DateUnit a, DateUnit b)
		{
			return a.Inner > b.Inner;
		}

		public static bool operator <=(DateUnit a, DateUnit b)
		{
			return a.Inner <= b.Inner;
		}

		public static bool operator >=(DateUnit a, DateUnit b)
		{
			return a.Inner >= b.Inner;
		}
	}
}
