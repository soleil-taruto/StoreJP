using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

		public void Test02()
		{
			List<string> dest = new List<string>(1000000);

			for (int c = 0; c < 1000000; c++)
			{
				dest.Add(UUIDTools.GetPermanentUniqueID());
			}
			File.WriteAllLines(SCommon.NextOutputPath() + ".txt", dest, Encoding.ASCII);
		}

		public void Test03()
		{
			// 時刻調整
			// -- ある秒のほぼ開始時点にする。
			{
				// memo: DateTime を == で比較するとミリ秒かナノ秒単位で比較されちゃうよ。

				SimpleDateTime dt = SimpleDateTime.Now();

				while (dt == SimpleDateTime.Now())
				{
					//Console.WriteLine("時刻調整中...");
					Thread.Sleep(100);
				}
				//Console.WriteLine("時刻調整完了");
			}

			List<string> dest = new List<string>(1000000);

			for (int c = 0; c < 65536 + 10; c++)
			{
				dest.Add(UUIDTools.GetPermanentUniqueID());
			}
			Thread.Sleep(1000);
			dest.Add("-- sleep 1s --");

			for (int c = 0; c < 10; c++)
			{
				dest.Add(UUIDTools.GetPermanentUniqueID());
			}
			Thread.Sleep(2000);
			dest.Add("-- sleep 2s --");

			for (int c = 0; c < 10; c++)
			{
				dest.Add(UUIDTools.GetPermanentUniqueID());
			}
			File.WriteAllLines(SCommon.NextOutputPath() + ".txt", dest, Encoding.ASCII);
		}
	}
}
