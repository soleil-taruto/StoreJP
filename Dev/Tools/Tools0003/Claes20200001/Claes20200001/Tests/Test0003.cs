using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Tools;
using Charlotte.Commons;
using System.IO;

namespace Charlotte.Tests
{
	public class Test0003
	{
		public void Test01()
		{
			Test01_a(0);
			Test01_a(1);
			Test01_a(2);
			Test01_a(3);
			Test01_a(10);
			Test01_a(30);
			Test01_a(100);
			Test01_a(300);
			Test01_a(1000);
			Test01_a(3000);
			Test01_a(1800000);
			Test01_a(1900000);
			Test01_a(1999998);
			Test01_a(1999999);
			Test01_a(2000000);
			Test01_a(2000001);
			Test01_a(2000002);
			Test01_a(2100000);
			Test01_a(2200000);
			Test01_a(3900000);
			Test01_a(4000000);
			Test01_a(4100000);
			Test01_a(5900000);
			Test01_a(6000000);
			Test01_a(6100000);
			Test01_a(7900000);
			Test01_a(8000000);
			Test01_a(8100000);

			ProcMain.WriteLog("OK!");
		}

		public void Test01_a(int fileSize)
		{
			ProcMain.WriteLog("TEST-0003-01 " + fileSize);

			byte[] fileData = SCommon.CRandom.GetBytes(fileSize);

			using (WorkingDir wd = new WorkingDir())
			{
				string file = wd.MakePath();

				File.WriteAllBytes(file, fileData);

				ProcMain.WriteLog("P-1");
				using (BlockBufferedFileReader reader = new BlockBufferedFileReader(file))
				{
					for (int index = 0; index < fileSize; index++)
						if (reader[(long)index] != fileData[index])
							throw null; // BUG !!!
				}

				ProcMain.WriteLog("P-2");
				using (BlockBufferedFileReader reader = new BlockBufferedFileReader(file))
				{
					for (int index = fileSize - 1; 0 <= index; index--)
						if (reader[(long)index] != fileData[index])
							throw null; // BUG !!!
				}

				ProcMain.WriteLog("P-3");
				using (BlockBufferedFileReader reader = new BlockBufferedFileReader(file))
				{
					const int BLOCK_SIZE = 30;

					for (int index = 0; index + BLOCK_SIZE <= fileSize; index++)
						for (int i = 0; i < BLOCK_SIZE; i++)
							if (reader[(long)(index + i)] != fileData[index + i])
								throw null; // BUG !!!
				}

				ProcMain.WriteLog("P-4");
				if (fileSize != 0)
				{
					using (BlockBufferedFileReader reader = new BlockBufferedFileReader(file))
					{
						for (int c = 0; c < 1000; c++)
						{
							int index = SCommon.CRandom.GetInt(fileSize);

							if (reader[(long)(index)] != fileData[index])
								throw null; // BUG !!!
						}
					}
				}
			}

			ProcMain.WriteLog("OK");
		}
	}
}
