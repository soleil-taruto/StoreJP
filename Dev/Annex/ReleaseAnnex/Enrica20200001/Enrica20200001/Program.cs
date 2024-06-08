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
			if (!Directory.Exists(Consts.SRC_ROOT_DIR))
				throw new Exception("no SRC_ROOT_DIR");

			SCommon.Batch(new string[]
			{
				"C:\\Factory\\DevTools\\runsub.exe /R \"" + Consts.SRC_ROOT_DIR + "\" Clean",
				"C:\\Factory\\DevTools\\runsub.exe /R \"" + Consts.SRC_ROOT_DIR + "\" Release",
				"TIMEOUT 2", // すぐ消えないように
			}
			, ""
			, SCommon.StartProcessWindowStyle_e.NORMAL
			);

			string[] dirs = Directory.GetDirectories(Consts.SRC_ROOT_DIR, "*", SearchOption.AllDirectories)
				.Where(v => Path.GetFileName(v).EqualsIgnoreCase("out"))
				.ToArray();

			CheckSrcOutDirs(dirs);

			string[] files = SCommon.Concat(dirs
				.Select(v => Directory.GetFiles(v))
				.ToArray())
				.OrderBy(SCommon.CompIgnoreCase)
				.ToArray();

			SCommon.DeletePath(Consts.OUTPUT_DIR);
			SCommon.CreateDir(Consts.OUTPUT_DIR);

			string wOutDir = Path.Combine(Consts.OUTPUT_DIR, Consts.OUTPUT_NAME);

			SCommon.CreateDir(wOutDir);

			foreach (string file in files)
			{
				string wFile = Path.Combine(wOutDir, Path.GetFileName(file));

				ProcMain.WriteLog("< " + file);
				ProcMain.WriteLog("> " + wFile);

				File.Move(file, wFile);

				ProcMain.WriteLog("done");
			}

#if false // 圧縮不要 @ 2024.6.1
			SCommon.Batch(new string[]
			{
				"C:\\Factory\\Tools\\z7.exe /C " + wOutDir,
			}
			, ""
			, SCommon.StartProcessWindowStyle_e.MINIMIZED
			);

			SCommon.DeletePath(wOutDir);
#endif

			ProcMain.WriteLog("done!");
		}

		private void CheckSrcOutDirs(string[] dirs)
		{
			foreach (string dir in dirs)
			{
				string[] files = Directory.GetFiles(dir);

				if (files.Length == 0)
					throw new Exception("ビルド未実行：" + dir);
			}
		}
	}
}
