using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using Charlotte.Commons;
using Charlotte.Tools;

namespace Charlotte.Tests
{
	public class Test0001
	{
		public void Test01()
		{
			SCommon.Pause();

			List<string> files = new List<string>();

			for (DateUnit date = DateUnit.SOFT_DATE_MIN; date <= DateUnit.SOFT_DATE_MAX; date++)
			{
				string file = Path.Combine(@"C:\temp\Databases\ApplicationNameABC123\DB\Daily"
					, (date.Year / 10) + "d"
					, date.Year.ToString()
					, (date.GetValue() / 100).ToString()
					, date.GetValue() + ".csv"
					);

				files.Add(file);

				SCommon.CreateDir(SCommon.ToParentPath(file));
				//File.WriteAllBytes(file, SCommon.EMPTY_BYTES);
				File.WriteAllText(file, "ABC123", Encoding.ASCII);
			}

			SCommon.Pause();
			files = null;
			GC.Collect();
		}

		public void Test02()
		{
			List<string> lines = new List<string>();

			// memo: DateUnit.DATE_MAX + 1 は 9999/01/01 になってしまうYo!

			for (DateUnit date = DateUnit.DATE_MIN; ; date++)
			{
				lines.Add(date + " ==> " + new JapaneseDateUnit(date));

				if (date == DateUnit.DATE_MAX)
					break;
			}
			File.WriteAllLines(SCommon.NextOutputPath() + ".txt", lines, SCommon.ENCODING_SJIS);
		}

		public void Test03()
		{
			Console.WriteLine(DateUnit.DATE_MAX + 1); // 9999/01/01

			{
				DateTimeUnit a = DateTimeUnit.CreateByValue(19991231235959);
				DateTimeUnit b = a;

				Console.WriteLine(a); // 1999/12/31 23:59:59
				Console.WriteLine(b); // 1999/12/31 23:59:59

				a++;

				Console.WriteLine(a); // 2000/01/01 00:00:00
				Console.WriteLine(b); // 1999/12/31 23:59:59

				b--;

				Console.WriteLine(a); // 2000/01/01 00:00:00
				Console.WriteLine(b); // 1999/12/31 23:59:58
			}
		}
	}
}
