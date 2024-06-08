using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Commons
{
	/// <summary>
	/// 日時の範囲
	/// -- 最小 1/1/1 00:00:00
	/// -- 最大 922337203/12/31 23:59:59
	/// </summary>
	public struct SimpleDateTime
	{
		private readonly long TimeStamp;

		public static SimpleDateTime Now()
		{
			return new SimpleDateTime(DateTime.Now);
		}

		public static SimpleDateTime FromSec(long sec)
		{
			return new SimpleDateTime(sec);
		}

		public static SimpleDateTime FromTimeStamp(long timeStamp)
		{
			return new SimpleDateTime(SCommon.TimeStampToSec.ToSec(timeStamp));
		}

		public SimpleDateTime(DateTime dateTime)
			: this(ToSec(dateTime))
		{ }

		private static long ToSec(DateTime dateTime)
		{
			long timeStamp =
				10000000000L * dateTime.Year +
				100000000L * dateTime.Month +
				1000000L * dateTime.Day +
				10000L * dateTime.Hour +
				100L * dateTime.Minute +
				dateTime.Second;

			return SCommon.TimeStampToSec.ToSec(timeStamp);
		}

		private SimpleDateTime(long sec)
		{
			this.TimeStamp = SCommon.TimeStampToSec.ToTimeStamp(sec);
		}

		public int Year
		{
			get
			{
				return (int)(this.TimeStamp / 10000000000);
			}
		}

		public int Month
		{
			get
			{
				return (int)((this.TimeStamp / 100000000) % 100);
			}
		}

		public int Day
		{
			get
			{
				return (int)((this.TimeStamp / 1000000) % 100);
			}
		}

		public int Hour
		{
			get
			{
				return (int)((this.TimeStamp / 10000) % 100);
			}
		}

		public int Minute
		{
			get
			{
				return (int)((this.TimeStamp / 100) % 100);
			}
		}

		public int Second
		{
			get
			{
				return (int)(this.TimeStamp % 100);
			}
		}

		private static readonly char[] DAY_OF_WEEK = new char[] { '月', '火', '水', '木', '金', '土', '日' };

		public char DayOfWeek
		{
			get
			{
				return DAY_OF_WEEK[(int)((SCommon.TimeStampToSec.ToSec(this.TimeStamp) / 86400) % 7)];
			}
		}

		public long ToSec()
		{
			return SCommon.TimeStampToSec.ToSec(this.TimeStamp);
		}

		public long ToTimeStamp()
		{
			return this.TimeStamp;
		}

		public DateTime ToDateTime()
		{
			// memo: @ 2023.8.30
			// DateTime に変換できる日時は 1/1/1 00:00:00 ～ 9999/12/31 23:59:59
			// それ以外は例外を投げる。

			// memo: @ 2023.8.31
			// 以下のように new DateTime(2023, 8, 31, 11, 19, 30) と作成すると Kind は DateTimeKind.Unspecified になる。
			// これは .ToLocalTime(), .ToUniversalTime() によって日時変更可能でこのとき Kind はそれぞれ DateTimeKind.Local, DateTimeKind.Utc になる。

			return new DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second);
		}

		public override string ToString()
		{
			return string.Format("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second);
		}

		public static SimpleDateTime operator ++(SimpleDateTime instance)
		{
			return instance + 1;
		}

		public static SimpleDateTime operator --(SimpleDateTime instance)
		{
			return instance - 1;
		}

		public static SimpleDateTime operator +(SimpleDateTime instance, long sec)
		{
			return new SimpleDateTime(instance.ToSec() + sec);
		}

		public static SimpleDateTime operator +(long sec, SimpleDateTime instance)
		{
			return new SimpleDateTime(instance.ToSec() + sec);
		}

		public static SimpleDateTime operator -(SimpleDateTime instance, long sec)
		{
			return new SimpleDateTime(instance.ToSec() - sec);
		}

		public static long operator -(SimpleDateTime a, SimpleDateTime b)
		{
			return a.ToSec() - b.ToSec();
		}

		private long GetValueForCompare()
		{
			return this.TimeStamp;
		}

		public static bool operator ==(SimpleDateTime a, SimpleDateTime b)
		{
			return a.GetValueForCompare() == b.GetValueForCompare();
		}

		public static bool operator !=(SimpleDateTime a, SimpleDateTime b)
		{
			return a.GetValueForCompare() != b.GetValueForCompare();
		}

		public override bool Equals(object another)
		{
			return another is SimpleDateTime && this == (SimpleDateTime)another;
		}

		public override int GetHashCode()
		{
			return this.GetValueForCompare().GetHashCode();
		}

		public static bool operator <(SimpleDateTime a, SimpleDateTime b)
		{
			return a.GetValueForCompare() < b.GetValueForCompare();
		}

		public static bool operator >(SimpleDateTime a, SimpleDateTime b)
		{
			return a.GetValueForCompare() > b.GetValueForCompare();
		}

		public static bool operator <=(SimpleDateTime a, SimpleDateTime b)
		{
			return a.GetValueForCompare() <= b.GetValueForCompare();
		}

		public static bool operator >=(SimpleDateTime a, SimpleDateTime b)
		{
			return a.GetValueForCompare() >= b.GetValueForCompare();
		}
	}
}
