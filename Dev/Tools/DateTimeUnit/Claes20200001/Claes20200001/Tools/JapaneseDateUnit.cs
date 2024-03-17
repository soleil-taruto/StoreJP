using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.Tools
{
	public class JapaneseDateUnit
	{
		private class Era_t
		{
			public DateUnit FirstDate;
			public string Name;
			public char Alphabet;

			public Era_t(int firstDate, string name, char alphabet)
			{
				this.FirstDate = DateUnit.CreateByValue(firstDate);
				this.Name = name;
				this.Alphabet = alphabet;
			}
		}

		private Era_t Era;
		private DateUnit Date;

		private static Era_t[] EraList = new Era_t[]
		{
			new Era_t(18680101, "明治", 'M'),
			new Era_t(19120730, "大正", 'T'),
			new Era_t(19261225, "昭和", 'S'),
			new Era_t(19890108, "平成", 'H'),
			new Era_t(20190501, "令和", 'R'),
		};

		public JapaneseDateUnit(DateUnit date)
		{
			int index;

			for (index = EraList.Length - 1; 0 <= index; index--)
				if (EraList[index].FirstDate <= date)
					break;

			this.Era = index == -1 ? null : EraList[index];
			this.Date = date;
		}

		public string EraName
		{
			get
			{
				if (this.Era == null)
					return "西暦";

				return this.Era.Name;
			}
		}

		public char EraAlphabet
		{
			get
			{
				if (this.Era == null)
					return '?';

				return this.Era.Alphabet;
			}
		}

		public int IntNen
		{
			get
			{
				if (this.Era == null)
					return this.Date.Year;

				return this.Date.Year - this.Era.FirstDate.Year + 1;
			}
		}

		public string Nen
		{
			get
			{
				int nen = this.IntNen;
				return nen == 1 ? "元" : nen.ToString();
			}
		}

		public int Month
		{
			get
			{
				return this.Date.Month;
			}
		}

		public int Day
		{
			get
			{
				return this.Date.Day;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}{1}年{2}月{3}日", this.EraName, this.Nen, this.Month, this.Day);
		}
	}
}
