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

			Main4(new ArgsReader(new string[] { "Dev", "J" }));
			//Main4(new ArgsReader(new string[] { "DevBin", "J" }));
			//Main4(new ArgsReader(new string[] { "DevLabo", "J" }));

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

		private string R_RootDir;
		private string W_RootDir;

		private void Main5(ArgsReader ar)
		{
			string devDirName = ar.NextArg();
			string alpha = ar.NextArg();

			ar.End();

			if (!SCommon.IsFairLocalPath(devDirName, -1))
				throw new Exception("Bad devDirName");

			if (!Regex.IsMatch(alpha, "^[A-Z]$"))
				throw new Exception("Bad alpha");

			R_RootDir = string.Format(Consts.R_ROOT_DIR_FORMAT, devDirName);
			W_RootDir = string.Format(Consts.W_ROOT_DIR_FORMAT, devDirName, alpha);

			ProcMain.WriteLog("< " + R_RootDir);
			ProcMain.WriteLog("> " + W_RootDir);

			if (!Directory.Exists(R_RootDir))
				throw new Exception("no R_RootDir");

			if (!Directory.Exists(W_RootDir))
				throw new Exception("no W_RootDir");

			SimpleDateTime wDirCrDT = new SimpleDateTime(new DirectoryInfo(W_RootDir).CreationTime);
			SimpleDateTime now = SimpleDateTime.Now();
			long wDirCrElpSec = now - wDirCrDT;

			ProcMain.WriteLog("wDirCrElpSec: " + wDirCrElpSec);

			if (Consts.CR_W_DIR_EXPIRE_SEC <= wDirCrElpSec)
			{
				ProcMain.WriteLog("Expired!");

				SCommon.DeletePath(W_RootDir);
				SCommon.CreateDir(W_RootDir);
			}

			SCommon.Batch(new string[] 
			{
				"ROBOCOPY \"" + R_RootDir + "\" \"" + W_RootDir + "\" /MIR",
			});

			ProcMain.WriteLog("done!");
		}
	}
}
