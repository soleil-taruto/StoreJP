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
using Charlotte.CSSolutions;

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

			Main4(new ArgsReader(new string[] { @"C:\Dev\Barebone\Game\Gattonero20200001", "Gattonero20200001", @"C:\temp\Game.exe" }));
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
			string rDir = SCommon.MakeFullPath(ar.NextArg());
			string name = ar.NextArg();
			string wFile = SCommon.MakeFullPath(ar.NextArg());

			ProcMain.WriteLog("< " + rDir);
			ProcMain.WriteLog("* " + name);
			ProcMain.WriteLog("> " + wFile);

			if (string.IsNullOrEmpty(rDir))
				throw new Exception("Bad rDir");

			if (!Directory.Exists(rDir))
				throw new Exception("no rDir");

			if (string.IsNullOrEmpty(name))
				throw new Exception("Bad name");

			if (string.IsNullOrEmpty(wFile))
				throw new Exception("Bad wFile");

			if (Directory.Exists(wFile))
				throw new Exception("Bad wFile");

			SCommon.DeletePath(wFile);
			File.WriteAllBytes(wFile, SCommon.EMPTY_BYTES); // テスト書き込み
			SCommon.DeletePath(wFile);
			SCommon.DeletePath(Consts.MID_DIR);
			SCommon.CopyDir(rDir, Consts.MID_DIR);

			CSSolution sol = new CSSolution(Consts.MID_DIR, name);

			//sol.Clean();
			sol.Confuse();
			sol.Build();

			File.Copy(sol.OutputFile, wFile);

			ProcMain.WriteLog("OK!");
		}
	}
}
