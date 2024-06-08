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
using Charlotte.Tools;

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

			Main4(new ArgsReader(new string[] { @"C:\home\TestData\WebpToPng\73a3806d-584a-4769-8b9e-70675f54dcc6.webp" }));
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
			// 外部依存チェック
			{
				if (!File.Exists(Consts.FFmpegExeFilePath))
					throw new Exception("no FFmpeg");
			}

			bool outputSameDirFlag = false;
			bool outputAndDeleteFlag = false;

			while (ar.HasArgs())
			{
				if (ar.ArgIs("/C"))
				{
					Console.WriteLine("+----------------+");
					Console.WriteLine("| 同じ場所に出力 |");
					Console.WriteLine("+----------------+");

					outputSameDirFlag = true;
					continue;
				}
				if (ar.ArgIs("/OAD"))
				{
					Console.WriteLine("+-------------------+");
					Console.WriteLine("| OUTPUT AND DELETE |");
					Console.WriteLine("+-------------------+");

					outputAndDeleteFlag = true;
					continue;
				}
				break;
			}
			string rFile;
			string wFile;

			if (ar.HasArgs())
			{
				rFile = ar.NextArg();
			}
			else
			{
				rFile = WDropTools.Run();

				if (rFile == null)
					return;
			}
			rFile = SCommon.MakeFullPath(rFile);

			ar.End();

			if (!Path.GetExtension(rFile).EqualsIgnoreCase(".webp"))
				throw new Exception("rFile is not .webp");

			if (outputSameDirFlag)
				wFile = SCommon.ChangeExt(rFile, ".png");
			else
				wFile = Path.Combine(SCommon.GetOutputDir(), Path.GetFileNameWithoutExtension(rFile) + ".png");

			Console.WriteLine("< " + rFile);
			Console.WriteLine("> " + wFile);

			if (!File.Exists(rFile))
				throw new Exception("no rFile");

			if (!IsFileFormat_Webp(rFile))
				throw new Exception("Bad rFile");

			if (Directory.Exists(wFile))
				throw new Exception("Bad wFile");

			using (WorkingDir wd = new WorkingDir())
			{
				string midFile1 = wd.MakePath() + ".webp";
				string midFile2 = wd.MakePath() + ".bmp";
				string midFile3 = wd.MakePath() + ".png";

				File.Copy(rFile, midFile1);

				if (!File.Exists(midFile1))
					throw new Exception("no midFile1");

				SCommon.Batch(new string[]
				{
					Consts.FFmpegExeFilePath + " -i " + midFile1 + " " + midFile2,
				});

				if (!File.Exists(midFile2))
					throw new Exception("Failed FFmpeg (no midFile2)");

				Canvas.LoadFromFile(midFile2).Save(midFile3);

				SCommon.DeletePath(wFile);
				File.Move(midFile3, wFile);
			}

			if (outputAndDeleteFlag)
				SCommon.DeletePath(rFile);

			Console.WriteLine("done!");
		}

		private bool IsFileFormat_Webp(string file)
		{
			// 参照元：
			// -- https://en.wikipedia.org/wiki/WebP
			//
			byte[] MAGIC_NUMBER = new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38 };
			//                                                         ~~~~~~~~~~
			//                                                         不定部分

			if (new FileInfo(file).Length < MAGIC_NUMBER.Length)
				return false;

			using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				byte[] data = SCommon.Read(reader, MAGIC_NUMBER.Length);

				// 不定部分
				data[4] = 0;
				data[5] = 0;
				data[6] = 0;
				data[7] = 0;

				if (SCommon.Comp(data, MAGIC_NUMBER, SCommon.Comp) != 0)
					return false;
			}
			return true;
		}
	}
}
