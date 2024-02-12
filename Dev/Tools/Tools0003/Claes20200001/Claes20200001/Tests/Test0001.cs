using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;
using Charlotte.Tools;

namespace Charlotte.Tests
{
	public class Test0001
	{
		public void Test01()
		{
			for (int c = 0; c < 1000000; c++)
			{
				Console.WriteLine(UUIDTools.GetPermanentUniqueID());
			}
		}
	}
}
