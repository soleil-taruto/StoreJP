using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	/// <summary>
	/// 日時
	/// 範囲：
	/// -- DateUnit.YEAR_MIN/1/1 00:00:00 ～ DateUnit.YEAR_MAX/12/31 23:59:59
	/// </summary>
	public struct DateTimeUnit
	{
		/// <summary>
		/// 最小の日時
		/// </summary>
		public static readonly DateTimeUnit DATETIME_MIN = new DateTimeUnit(DateUnit.DATE_MIN, 0, 0, 0);

		/// <summary>
		/// 最大の日時
		/// </summary>
		public static readonly DateTimeUnit DATETIME_MAX = new DateTimeUnit(DateUnit.DATE_MAX, 23, 59, 59);

		/// <summary>
		/// 最小の日時(ゆるい制限)
		/// </summary>
		public static readonly DateTimeUnit SOFT_DATETIME_MIN = new DateTimeUnit(DateUnit.SOFT_DATE_MIN, 0, 0, 0);

		/// <summary>
		/// 最大の日時(ゆるい制限)
		/// </summary>
		public static readonly DateTimeUnit SOFT_DATETIME_MAX = new DateTimeUnit(DateUnit.SOFT_DATE_MAX, 23, 59, 59);

		/// <summary>
		/// 保持している日時
		/// </summary>
		private SimpleDateTime Inner;

		/// <summary>
		/// 日時からインスタンスを作成する。
		/// </summary>
		/// <param name="date">日付</param>
		/// <param name="h">時</param>
		/// <param name="i">分</param>
		/// <param name="s">秒</param>
		public DateTimeUnit(DateUnit date, int h, int i, int s)
		{
			// date
			h = SCommon.ToRange(h, 0, 23);
			i = SCommon.ToRange(i, 0, 59);
			s = SCommon.ToRange(s, 0, 59);

			int y = date.Year;
			int m = date.Month;
			int d = date.Day;

			this.Inner = SimpleDateTime.FromTimeStamp(
				y * 10000000000L +
				m * 100000000L +
				d * 1000000L +
				h * 10000L +
				i * 100L +
				s
				);
		}

		/// <summary>
		/// 日時からインスタンスを作成する。
		/// </summary>
		/// <param name="y">年</param>
		/// <param name="m">月</param>
		/// <param name="d">日</param>
		/// <param name="h">時</param>
		/// <param name="i">分</param>
		/// <param name="s">秒</param>
		public DateTimeUnit(int y, int m, int d, int h, int i, int s)
			: this(new DateUnit(y, m, d), h, i, s)
		{ }

		/// <summary>
		/// 日時からインスタンスを作成する。
		/// 日時の形式：
		/// -- YYYYMMDDHHIISS
		/// </summary>
		/// <param name="dateTime">日時</param>
		/// <returns>日時のインスタンス</returns>
		public static DateTimeUnit CreateByValue(long dateTime)
		{
			int y = (int)(dateTime / 10000000000);
			int m = (int)((dateTime / 100000000) % 100);
			int d = (int)((dateTime / 1000000) % 100);
			int h = (int)((dateTime / 10000) % 100);
			int i = (int)((dateTime / 100) % 100);
			int s = (int)(dateTime % 100);

			return new DateTimeUnit(y, m, d, h, i, s);
		}

		/// <summary>
		/// 現在の日時を取得する。
		/// </summary>
		/// <returns>現在の日時</returns>
		public static DateTimeUnit GetNow()
		{
			return CreateByValue(SimpleDateTime.Now().ToTimeStamp());
		}

		/// <summary>
		/// 年
		/// 範囲：
		/// -- DateUnit.YEAR_MIN ～ DateUnit.YEAR_MAX
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
		/// 時
		/// 範囲：
		/// -- 0 ～ 23
		/// </summary>
		public int Hour
		{
			get
			{
				return this.Inner.Hour;
			}
		}

		/// <summary>
		/// 分
		/// 範囲：
		/// -- 0 ～ 59
		/// </summary>
		public int Minute
		{
			get
			{
				return this.Inner.Minute;
			}
		}

		/// <summary>
		/// 秒
		/// 範囲：
		/// -- 0 ～ 59
		/// </summary>
		public int Second
		{
			get
			{
				return this.Inner.Second;
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
		/// 日時の値を取得する。
		/// 形式：
		/// -- YYYYMMDDHHIISS
		/// </summary>
		public long GetValue()
		{
			return this.Inner.ToTimeStamp();
		}

		/// <summary>
		/// 日時の文字列を取得する。
		/// </summary>
		/// <returns>日時</returns>
		public override string ToString()
		{
			return string.Format("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second);
		}

		/// <summary>
		/// 時刻を除去して日付に変換する。
		/// </summary>
		/// <returns>日付</returns>
		public DateUnit WithoutTime()
		{
			return DateUnit.CreateByValue((int)(this.GetValue() / 1000000));
		}

		public static DateTimeUnit operator ++(DateTimeUnit instance)
		{
			return instance + 1;
		}

		public static DateTimeUnit operator --(DateTimeUnit instance)
		{
			return instance - 1;
		}

		public static DateTimeUnit operator +(DateTimeUnit instance, long sec)
		{
			return CreateByValue((instance.Inner + sec).ToTimeStamp());
		}

		public static DateTimeUnit operator +(long sec, DateTimeUnit instance)
		{
			return CreateByValue((instance.Inner + sec).ToTimeStamp());
		}

		public static DateTimeUnit operator -(DateTimeUnit instance, long sec)
		{
			return CreateByValue((instance.Inner - sec).ToTimeStamp());
		}

		public static long operator -(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner - b.Inner;
		}

		public static bool operator ==(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner == b.Inner;
		}

		public static bool operator !=(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner != b.Inner;
		}

		public override bool Equals(object another)
		{
			return another is DateTimeUnit && this == (DateTimeUnit)another;
		}

		public override int GetHashCode()
		{
			return this.Inner.GetHashCode();
		}

		public static bool operator <(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner < b.Inner;
		}

		public static bool operator >(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner > b.Inner;
		}

		public static bool operator <=(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner <= b.Inner;
		}

		public static bool operator >=(DateTimeUnit a, DateTimeUnit b)
		{
			return a.Inner >= b.Inner;
		}
	}
}
