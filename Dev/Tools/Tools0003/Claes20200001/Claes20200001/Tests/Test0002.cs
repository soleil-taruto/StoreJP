using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Tools;

namespace Charlotte.Tests
{
	public class Test0002
	{
		public void Test01()
		{
			Test01_a(1, "A");
			Test01_a(2, "B");
			Test01_a(3, "C");
			Test01_a(26, "Z");
			Test01_a(27, "AA");
			Test01_a(28, "AB");
			Test01_a(29, "AC");
			Test01_a(52, "AZ");
			Test01_a(53, "BA");
			Test01_a(54, "BB");
			Test01_a(55, "BC");
			Test01_a(702, "ZZ");
			Test01_a(703, "AAA");
			Test01_a(704, "AAB");
			Test01_a(705, "AAC");
			Test01_a(18278, "ZZZ");
			Test01_a(18279, "AAAA");

			Console.WriteLine("OK! (TEST-0002-01)");
		}

		private void Test01_a(int index, string expectName)
		{
			string name = ExcelColumnPosition.ToName(index);

			if (name != expectName)
				throw null; // BUG !!
		}

		public void Test02()
		{
			for (int index = 1; index <= 1000000; index++)
			{
				string name = ExcelColumnPosition.ToName(index);
				int indexFN = ExcelColumnPosition.ToIndex(name);

				//if (index % 1000 == 0) Console.WriteLine(string.Join(" ==> ", index, name, indexFN)); // cout

				if (index != indexFN)
					throw null; // BUG !!
			}
			Console.WriteLine("OK! (TEST-0002-02)");
		}
	}
}
