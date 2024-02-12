using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public class CsvFileDBTable
	{
		// 制約：
		// -- 全ての行は1列以上の長さがなければならない。
		// -- 1列目は(重複ナシの)IDとする。
		// -- IDは空文字列不可
		// -- 追加または更新された行は1行目の前に挿入(移動)する。

		// 注意：
		// -- 削除バッファ優先 -- 削除バッファ・追加または更新バッファ両方に同じIDがあった場合、削除として扱う。

		private static int BUFFER_SIZE_MAX = 1000;
		private static int MEMORY_LOAD_MAX = 50000000; // 50 MB

		public static void DEBUG_SetBufferSizeMax(int value)
		{
			BUFFER_SIZE_MAX = value;
		}

		public static void DEBUG_SetMemoryLoadMax(int value)
		{
			MEMORY_LOAD_MAX = value;
		}

		private static int RowsToMemoryLoad(string[][] rows)
		{
			return rows.Length * 100 + rows.Sum(row => row.Length * 100 + row.Sum(v => v.Length * 2)); // rough value
		}

		private static int RowToMemoryLoad(string[] row)
		{
			return RowsToMemoryLoad(new string[][] { row });
		}

		private string FilePath;

		/// <summary>
		/// 削除されたID
		/// 注意：バッファ内はID重複あり
		/// </summary>
		private string DeleteBufferFilePath
		{
			get
			{
				return this.FilePath + "_DeleteBuffer.csv";
			}
		}

		/// <summary>
		/// 追加または更新されたレコード
		/// 注意：バッファ内はID重複あり(後の方のレコードが有効)
		/// </summary>
		private string UpdateBufferFilePath
		{
			get
			{
				return this.FilePath + "_UpdateBuffer.csv";
			}
		}

		private int DeleteBufferSize;
		private int DeleteMemoryLoad;
		private int UpdateBufferSize;
		private int UpdateMemoryLoad;

		public CsvFileDBTable(string file)
		{
			this.FilePath = SCommon.MakeFullPath(file);

			// ----

			if (!File.Exists(this.FilePath))
				File.WriteAllBytes(this.FilePath, SCommon.EMPTY_BYTES);

			if (!File.Exists(this.DeleteBufferFilePath))
				File.WriteAllBytes(this.DeleteBufferFilePath, SCommon.EMPTY_BYTES);

			if (!File.Exists(this.UpdateBufferFilePath))
				File.WriteAllBytes(this.UpdateBufferFilePath, SCommon.EMPTY_BYTES);

			using (CsvFileReader reader = new CsvFileReader(this.DeleteBufferFilePath))
			{
				string[][] rows = reader.ReadToEnd();

				this.DeleteBufferSize = rows.Length;
				this.DeleteMemoryLoad = RowsToMemoryLoad(reader.ReadToEnd());
			}

			using (CsvFileReader reader = new CsvFileReader(this.UpdateBufferFilePath))
			{
				string[][] rows = reader.ReadToEnd();

				this.UpdateBufferSize = rows.Length;
				this.UpdateMemoryLoad = RowsToMemoryLoad(reader.ReadToEnd());
			}
		}

		/// <summary>
		/// 参照のみの全件走査
		/// 削除・更新を行う場合は FilterAll を使用すること。
		/// </summary>
		/// <param name="reaction">行リアクション</param>
		public void ForEach(Predicate<string[]> reaction)
		{
			if (reaction == null)
				throw new Exception("Bad reaction");

			HashSet<string> deletedOrKnownIDs = new HashSet<string>();
			string[][] addedOrUpdatedRows;

			using (CsvFileReader reader = new CsvFileReader(this.DeleteBufferFilePath))
			{
				foreach (string id in reader.ReadToEnd().Select(row => row[0]))
				{
					deletedOrKnownIDs.Add(id);
				}
			}

			using (CsvFileReader reader = new CsvFileReader(this.UpdateBufferFilePath))
			{
				addedOrUpdatedRows = reader.ReadToEnd();
			}

			foreach (string[] row in addedOrUpdatedRows.Reverse()) // 最後の更新を優先するため、最後から読み込む。
			{
				if (deletedOrKnownIDs.Contains(row[0]))
					continue;

				if (!reaction(row))
					return;

				deletedOrKnownIDs.Add(row[0]);
			}

			using (CsvFileReader reader = new CsvFileReader(this.FilePath))
			{
				for (; ; )
				{
					string[] row = reader.ReadRow();

					if (row == null)
						break;

					if (deletedOrKnownIDs.Contains(row[0]))
						continue;

					if (!reaction(row))
						break;
				}
			}
		}

		/// <summary>
		/// 削除・更新を伴う全件走査
		/// 行フィルタ：
		/// -- 何もしない場合 == 引数をそのまま返す。
		/// -- 更新する場合 == 新しい行を返す。-- 1列目(ID)を変更してはならない。
		/// -- 削除する場合 == nullを返す。
		/// </summary>
		/// <param name="filter">行フィルタ</param>
		public void FilterAll(Func<string[], string[]> filter)
		{
			if (filter == null)
				throw new Exception("Bad filter");

			using (WorkingDir wd = new WorkingDir())
			{
				string midFile = wd.MakePath();

				using (CsvFileWriter writer = new CsvFileWriter(midFile))
				{
					this.ForEach(row =>
					{
						string[] newRow = filter(row);

						if (newRow != null)
						{
							if (
								newRow.Length < 1 ||
								newRow.Any(v => v == null) ||
								newRow[0] != row[0] // 1列目(ID)の不一致
								)
								throw new Exception("Bad newRow");

							writer.WriteRow(newRow);
						}
						return true;
					});
				}

				SCommon.DeletePath(this.FilePath);
				File.Move(midFile, this.FilePath);
			}

			File.WriteAllBytes(this.DeleteBufferFilePath, SCommon.EMPTY_BYTES);
			File.WriteAllBytes(this.UpdateBufferFilePath, SCommon.EMPTY_BYTES);

			this.DeleteBufferSize = 0;
			this.DeleteMemoryLoad = 0;
			this.UpdateBufferSize = 0;
			this.UpdateMemoryLoad = 0;
		}

		public List<string[]> Search_MLM(Predicate<string[]> match, int memoryLoadMax, out bool overflow)
		{
			if (
				match == null ||
				memoryLoadMax < 1 || SCommon.IMAX < memoryLoadMax
				)
				throw new Exception("Bad params");

			List<string[]> dest = new List<string[]>();
			int memoryLoad = 0;
			bool wOverflow = false;

			this.ForEach(row =>
			{
				if (match(row)) // ? 検索対象
				{
					memoryLoad += RowToMemoryLoad(row);

					if (memoryLoadMax < memoryLoad)
					{
						wOverflow = true;
						return false;
					}
					dest.Add(row);
				}
				return true;
			});

			overflow = wOverflow;
			return dest;
		}

		public List<string[]> Search(Predicate<string[]> match, int limit, out bool overflow)
		{
			if (
				match == null ||
				limit < 1 || SCommon.IMAX < limit
				)
				throw new Exception("Bad params");

			List<string[]> dest = new List<string[]>();
			bool wOverflow = false;

			this.ForEach(row =>
			{
				if (match(row)) // ? 検索対象
				{
					if (limit <= dest.Count)
					{
						wOverflow = true;
						return false;
					}
					dest.Add(row);
				}
				return true;
			});

			overflow = wOverflow;
			return dest;
		}

		public List<string[]> Search(Predicate<string[]> match, Comparison<string[]> comp, int limit, out int count)
		{
			if (
				match == null ||
				comp == null ||
				limit < 1 || SCommon.IMAX < limit
				)
				throw new Exception("Bad params");

			int DEST_MAX = Math.Max(limit + limit / 2, 100); // rough limit

			List<string[]> dest = new List<string[]>();
			int wCount = 0;

			this.ForEach(row =>
			{
				if (match(row)) // ? 検索対象
				{
					if (DEST_MAX < dest.Count)
					{
						dest.Sort(comp);
						dest.RemoveRange(limit, dest.Count - limit);
					}
					dest.Add(row);
					wCount++;
				}
				return true;
			});

			dest.Sort(comp);

			if (limit < dest.Count)
				dest.RemoveRange(limit, dest.Count - limit);

			count = wCount;
			return dest;
		}

		public List<string[]> Search(Predicate<string[]> match, Comparison<string[]> comp, int offset, int limit, out int count)
		{
			if (
				match == null ||
				comp == null ||
				offset < 0 || SCommon.IMAX < offset ||
				limit < 1 || SCommon.IMAX - offset < limit
				)
				throw new Exception("Bad params");

			List<string[]> dest = new List<string[]>();
			int wCount = 0;

			using (WorkingDir wd = new WorkingDir())
			{
				string midFile = wd.MakePath();

				using (CsvFileWriter writer = new CsvFileWriter(midFile))
				{
					this.ForEach(row =>
					{
						if (match(row)) // ? 検索対象
						{
							writer.WriteRow(row);
							wCount++;
						}
						return true;
					});
				}

				CsvFileSorter.Sort(midFile, comp);

				using (CsvFileReader reader = new CsvFileReader(midFile))
				{
					for (int index = 0; index < wCount; index++)
					{
						string[] row = reader.ReadRow();

						if (row == null)
							throw null; // never

						if (index < offset) // ? 出力開始位置の前
							continue;

						dest.Add(row);

						if (limit <= dest.Count) // ? 出力件数に達した。
							break;
					}
				}
			}

			count = wCount;
			return dest;
		}

		public void Search(Predicate<string[]> match, Comparison<string[]> comp, Predicate<string[]> reaction)
		{
			if (
				match == null ||
				comp == null ||
				reaction == null
				)
				throw new Exception("Bad params");

			using (WorkingDir wd = new WorkingDir())
			{
				string midFile = wd.MakePath();

				using (CsvFileWriter writer = new CsvFileWriter(midFile))
				{
					this.ForEach(row =>
					{
						if (match(row))
							writer.WriteRow(row);

						return true;
					});
				}

				CsvFileSorter.Sort(midFile, comp);

				using (CsvFileReader reader = new CsvFileReader(midFile))
				{
					for (; ; )
					{
						string[] row = reader.ReadRow();

						if (row == null)
							break;

						if (!reaction(row))
							break;
					}
				}
			}
		}

		/// <summary>
		/// 行の削除を行う。
		/// 大量の削除には向かない。
		/// 大量削除は FilterAll を検討すること。
		/// </summary>
		/// <param name="id">削除する行のID</param>
		public void Delete(string id)
		{
			if (string.IsNullOrEmpty(id))
				throw new Exception("Bad id");

			using (CsvFileWriter writer = new CsvFileWriter(this.DeleteBufferFilePath, true))
			{
				writer.WriteCell(id);
				writer.EndRow();
			}

			this.DeleteBufferSize += 1;
			this.DeleteMemoryLoad += RowToMemoryLoad(new string[] { id });

			if (
				BUFFER_SIZE_MAX < this.DeleteBufferSize ||
				MEMORY_LOAD_MAX < this.DeleteMemoryLoad
				)
				this.Flush();
		}

		/// <summary>
		/// 行の追加または更新を行う。
		/// 追加または更新された行は1行目の前に追加(移動)される。
		/// 大量の追加・更新には向かない。
		/// 大量追加は BulkInsert 大量更新は FilterAll を検討すること。
		/// </summary>
		/// <param name="row">追加または更新する行</param>
		public void AddOrUpdate(string[] row)
		{
			if (
				row == null ||
				row.Length < 1 || // 1列以上必要(1列目は(重複ナシの)ID)
				row.Any(v => v == null) ||
				row[0] == "" // IDは空文字列不可
				)
				throw new Exception("Bad row");

			if (1 <= this.DeleteBufferSize) // 削除バッファが優先であるため！
				this.Flush();

			using (CsvFileWriter writer = new CsvFileWriter(this.UpdateBufferFilePath, true))
			{
				writer.WriteRow(row);
			}

			this.UpdateBufferSize += 1;
			this.UpdateMemoryLoad += RowToMemoryLoad(row);

			if (
				BUFFER_SIZE_MAX < this.UpdateBufferSize ||
				MEMORY_LOAD_MAX < this.UpdateMemoryLoad
				)
				this.Flush();
		}

		private void Flush()
		{
			// ? バッファ無し -> フラッシュ不要
			if (
				this.DeleteBufferSize == 0 &&
				this.UpdateBufferSize == 0
				)
				return;

			using (WorkingDir wd = new WorkingDir())
			{
				string midFile = wd.MakePath();

				using (CsvFileWriter writer = new CsvFileWriter(midFile))
				{
					this.ForEach(row =>
					{
						writer.WriteRow(row);
						return true;
					});
				}

				SCommon.DeletePath(this.FilePath);
				File.Move(midFile, this.FilePath);
			}

			File.WriteAllBytes(this.DeleteBufferFilePath, SCommon.EMPTY_BYTES);
			File.WriteAllBytes(this.UpdateBufferFilePath, SCommon.EMPTY_BYTES);

			this.DeleteBufferSize = 0;
			this.DeleteMemoryLoad = 0;
			this.UpdateBufferSize = 0;
			this.UpdateMemoryLoad = 0;
		}

		public void Sort(Comparison<string[]> comp)
		{
			if (comp == null)
				throw new Exception("Bad comp");

			this.Flush();
			CsvFileSorter.Sort(this.FilePath, comp);
		}

		public void Truncate()
		{
			File.WriteAllBytes(this.FilePath, SCommon.EMPTY_BYTES);
			File.WriteAllBytes(this.DeleteBufferFilePath, SCommon.EMPTY_BYTES);
			File.WriteAllBytes(this.UpdateBufferFilePath, SCommon.EMPTY_BYTES);

			this.DeleteBufferSize = 0;
			this.DeleteMemoryLoad = 0;
			this.UpdateBufferSize = 0;
			this.UpdateMemoryLoad = 0;
		}

		public void BulkInsert(Func<string[]> reader)
		{
			if (reader == null)
				throw new Exception("Bad reader");

			this.Flush();

			using (CsvFileWriter writer = new CsvFileWriter(this.FilePath, true))
			{
				for (; ; )
				{
					string[] row = reader();

					if (row == null) // 読み込み終了
						break;

					// IDの重複はチェックしない。

					if (
						row.Length < 1 || // 1列以上必要(1列目は(重複ナシの)ID)
						row.Any(v => v == null) ||
						row[0] == "" // IDは空文字列不可
						)
						throw new Exception("Bad row");

					writer.WriteRow(row);
				}
			}
		}
	}
}
