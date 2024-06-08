using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Charlotte.Commons;

namespace Charlotte
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcMain.CUIMain(new Program().Main2);
		}

		private void Main2(ArgsReader ar)
		{
			if (ProcMain.DEBUG)
			{
				Main3();
			}
			else
			{
				Main4(ar);
			}
			SCommon.OpenOutputDirIfCreated();
		}

		private void Main3()
		{
			// テスト系 -- リリース版では使用しない。
#if DEBUG
			// -- choose one --

			Main4(new ArgsReader(new string[] { }));
			//Main4(new ArgsReader(new string[] { }));
			//Main4(new ArgsReader(new string[] { }));

			// --
#endif
			SCommon.Pause();
		}

		private void Main4(ArgsReader ar)
		{
			try
			{
				Main5(ar);
			}
			catch (Exception ex)
			{
				ProcMain.WriteLog(ex);

				MessageBox.Show("" + ex, Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (エラーによりプログラムを終了します)");
				//Console.ReadLine();
			}
		}

		private void Main5(ArgsReader ar)
		{
			SCommon.DeletePath(Consts.FIXED_TEMP_DIR);
			SCommon.CreateDir(Consts.FIXED_TEMP_DIR);

			string dir1 = SCommon.MakeFullPath(ar.NextArg());
			string dir2 = SCommon.MakeFullPath(ar.NextArg());

			if (!Directory.Exists(dir1))
				throw new Exception("no dir1");

			if (!Directory.Exists(dir2))
				throw new Exception("no dir2");

			string[] files1 = Directory.GetFiles(dir1, "*", SearchOption.AllDirectories)
				.Select(file => SCommon.ChangeRoot(file, dir1))
				.ToArray();

			string[] files2 = Directory.GetFiles(dir2, "*", SearchOption.AllDirectories)
				.Select(file => SCommon.ChangeRoot(file, dir2))
				.ToArray();

			List<string>[] merged = SCommon.GetMerge(files1, files2, SCommon.CompIgnoreCase);

			foreach (string file in merged[0])
			{
				Console.WriteLine("<L>" + file);
			}
			foreach (string file in merged[3])
			{
				Console.WriteLine("<R>" + file);
			}
			foreach (string file in merged[1])
			{
				string file1 = Path.Combine(dir1, file);
				string file2 = Path.Combine(dir2, file);

				if (!IsSameFile(file1, file2))
				{
					Console.WriteLine("<D>" + file);

					FileInfo info1 = new FileInfo(file1);
					FileInfo info2 = new FileInfo(file2);

					if (
						Consts.COMPARE_FILE_SIZE_MAX < info1.Length ||
						Consts.COMPARE_FILE_SIZE_MAX < info2.Length
						)
					{
						Console.WriteLine("ファイルが大きすぎるため比較は行いません。");
						Console.WriteLine(string.Format("< {0} {1}", GetFileHash(file1), info1.Length));
						Console.WriteLine(string.Format("> {0} {1}", GetFileHash(file2), info2.Length));
					}
					else
					{
						SCommon.DeletePath(Consts.FILE_FOR_COMP_01);
						SCommon.DeletePath(Consts.FILE_FOR_COMP_02);
						SCommon.DeletePath(Consts.COMP_STDOUT_FILE);

						File.Copy(file1, Consts.FILE_FOR_COMP_01);
						File.Copy(file2, Consts.FILE_FOR_COMP_02);

						ResolveEncoding_FileForComp(Consts.FILE_FOR_COMP_01);
						ResolveEncoding_FileForComp(Consts.FILE_FOR_COMP_02);

						SCommon.Batch(new string[]
						{
							string.Format("fc.exe \"{0}\" \"{1}\" > \"{2}\""
								, Consts.FILE_FOR_COMP_01
								, Consts.FILE_FOR_COMP_02
								, Consts.COMP_STDOUT_FILE
								),
						}
						, Consts.FIXED_TEMP_DIR
						, SCommon.StartProcessWindowStyle_e.MINIMIZED
						);

						using (StreamReader reader = new StreamReader(Consts.COMP_STDOUT_FILE, SCommon.ENCODING_SJIS))
						{
							for (; ; )
							{
								string line = reader.ReadLine(); // 128文字目で改行が入るようなので、長すぎる行を読み込むことは無いだろう。

								if (line == null)
									break;

								line = SCommon.ToJString(line, true, true, true, true);

								Console.WriteLine(line);
							}
						}
					}
				}
			}
		}

		private bool IsSameFile(string file1, string file2)
		{
			using (FileStream reader1 = new FileStream(file1, FileMode.Open, FileAccess.Read))
			using (FileStream reader2 = new FileStream(file2, FileMode.Open, FileAccess.Read))
			{
				long fileSize = reader1.Length;

				if (fileSize != reader2.Length)
					return false;

				for (long index = 0; index < fileSize; index++)
				{
					int chr1 = reader1.ReadByte();
					int chr2 = reader2.ReadByte();

					if (chr1 != chr2)
						return false;
				}
			}
			return true;
		}

		private void ResolveEncoding_FileForComp(string file)
		{
			byte[] fileData = File.ReadAllBytes(file);
			string text;

			// ? has UTF-8 BOM -> UTF-8
			if (
				fileData.Length >= 3 &&
				fileData[0] == 0xef &&
				fileData[1] == 0xbb &&
				fileData[2] == 0xbf
				)
			{
				text = SCommon.UTF8Conv.ToJString(fileData);
			}
			else // SJIS
			{
				text = SCommon.ToJString(fileData, true, true, true, true);
			}
			fileData = SCommon.ENCODING_SJIS.GetBytes(text);
			File.WriteAllBytes(file, fileData);
		}

		private string GetFileHash(string file)
		{
			return SCommon.Hex.I.GetString(SCommon.GetPart(SCommon.GetSHA512File(file), 0, 16));
		}
	}
}
