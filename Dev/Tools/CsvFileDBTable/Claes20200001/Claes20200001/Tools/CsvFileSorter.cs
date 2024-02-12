using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class CsvFileSorter
	{
		private static int MEMORY_LOAD_MAX = 200000000; // 200 MB

		public static void DEBUG_SetMemoryLoadMax(int value)
		{
			MEMORY_LOAD_MAX = value;
		}

		public static List<int> DEBUG_LastRowCountList = new List<int>();

		private static int RowToMemoryLoad(string[] row)
		{
			return 100 + row.Length * 100 + row.Sum(v => v.Length) * 2; // rough value
		}

		public static void Sort(string file, Comparison<string[]> comp)
		{
			Sort(file, file, comp);
		}

		public static void Sort(string rFile, string wFile, Comparison<string[]> comp)
		{
			rFile = SCommon.MakeFullPath(rFile);
			wFile = SCommon.MakeFullPath(wFile);

			if (!File.Exists(rFile))
				throw new Exception("no rFile");

			if (Directory.Exists(wFile))
				throw new Exception("Bad wFile");

			if (comp == null)
				throw new Exception("Bad comp");

			using (WorkingDir wd = new WorkingDir())
			{
				Queue<string> q = new Queue<string>();

				DEBUG_LastRowCountList.Clear();

				using (CsvFileReader reader = new CsvFileReader(rFile))
				{
					for (; ; )
					{
						List<string[]> rows = new List<string[]>();
						string[] row;
						int memoryLoad = 0;

						for (; ; )
						{
							row = reader.ReadRow();

							if (row == null)
								break;

							rows.Add(row);
							memoryLoad += RowToMemoryLoad(row);

							if (MEMORY_LOAD_MAX < memoryLoad)
								break;
						}
						if (1 <= rows.Count)
						{
							rows.Sort(comp);

							string midFile = wd.MakePath();

							using (CsvFileWriter writer = new CsvFileWriter(midFile))
							{
								writer.WriteRows(rows);
							}
							q.Enqueue(midFile);

							DEBUG_LastRowCountList.Add(rows.Count);
						}
						if (row == null)
							break;
					}
				}

				if (q.Count == 0)
				{
					File.WriteAllBytes(wFile, SCommon.EMPTY_BYTES);
				}
				else
				{
					while (2 <= q.Count)
					{
						string midFile1 = q.Dequeue();
						string midFile2 = q.Dequeue();
						string midFile3 = wd.MakePath();

						using (CsvFileReader reader1 = new CsvFileReader(midFile1))
						using (CsvFileReader reader2 = new CsvFileReader(midFile2))
						using (CsvFileWriter writer = new CsvFileWriter(midFile3))
						{
							string[] row1 = reader1.ReadRow();
							string[] row2 = reader2.ReadRow();

							while (row1 != null && row2 != null)
							{
								int ret = comp(row1, row2);

								if (ret <= 0)
								{
									writer.WriteRow(row1);
									row1 = reader1.ReadRow();
								}
								if (0 <= ret)
								{
									writer.WriteRow(row2);
									row2 = reader2.ReadRow();
								}
							}
							while (row1 != null)
							{
								writer.WriteRow(row1);
								row1 = reader1.ReadRow();
							}
							while (row2 != null)
							{
								writer.WriteRow(row2);
								row2 = reader2.ReadRow();
							}
						}
						q.Enqueue(midFile3);
					}
					SCommon.DeletePath(wFile);
					File.Move(q.Dequeue(), wFile);
				}
			}
		}
	}
}
