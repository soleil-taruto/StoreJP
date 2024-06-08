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

			//Main4(new ArgsReader(new string[] { @"C:\home\HPStore", "2" }));
			Main4(new ArgsReader(new string[] { @"C:\home\HPStore", "-", @"C:\home\HPStore\Toolkit" }));

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

				//MessageBox.Show("" + ex, ProcMain.APP_TITLE + " / エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (エラーによりプログラムを終了します)");
				//Console.ReadLine();
			}
		}

		private string[] NoCopyDirs;

		private void Main5(ArgsReader ar)
		{
			string rDir = SCommon.MakeFullPath(ar.NextArg());
			int depth;

			if (ar.ArgIs("-"))
			{
				depth = SCommon.IMAX;
				NoCopyDirs = ar.TrailArgs().Select(v => SCommon.MakeFullPath(v)).ToArray();
			}
			else
			{
				depth = int.Parse(ar.NextArg());
				NoCopyDirs = new string[0];
			}
			ar.End();

			if (!Directory.Exists(rDir))
				throw new Exception("no rDir");

			if (depth < 0 || SCommon.IMAX < depth)
				throw new Exception("Bad depth");

			foreach (string noCopyDir in NoCopyDirs)
				if (!Directory.Exists(noCopyDir))
					throw new Exception("no noCopyDir: " + noCopyDir);

			string wLocalName = Path.GetFileName(rDir);

			if (string.IsNullOrEmpty(wLocalName))
				wLocalName = "_Root";

			string wDir = Path.Combine(SCommon.GetOutputDir(), wLocalName);

			Console.WriteLine("< " + rDir);
			Console.WriteLine("> " + wDir);
			Console.WriteLine("D " + depth);
			Console.WriteLine("N " + NoCopyDirs.Length);

			CopyDepth(rDir, wDir, depth);

			Console.WriteLine("done");
		}

		private bool IsNoCopyDir(string dir)
		{
			return NoCopyDirs.Any(v => SCommon.EqualsIgnoreCase(v, dir));
		}

		private void CopyDepth(string rDir, string wDir, int depth)
		{
			SCommon.CreateDir(wDir);

			if (1 <= depth && !IsNoCopyDir(rDir)) // ? コピーする深さ && コピーしないディレクトリではない。-> コピーする。
			{
				foreach (string dir in Directory.GetDirectories(rDir))
					CopyDepth(dir, Path.Combine(wDir, Path.GetFileName(dir)), depth - 1);

				foreach (string file in Directory.GetFiles(rDir))
					File.Copy(file, Path.Combine(wDir, Path.GetFileName(file)));
			}
			else // ? コピーしない深さ || コピーしないディレクトリ -> ツリー情報ファイルを作成する。
			{
				string treeFile = Path.Combine(wDir, "_Tree.txt");
				string[] treeFileData = MakeTreeFileData(rDir);

				File.WriteAllLines(treeFile, treeFileData, Encoding.UTF8);
			}
		}

		private string[] MakeTreeFileData(string targDir)
		{
			string[] paths = Directory.GetDirectories(targDir, "*", SearchOption.AllDirectories)
				.Concat(Directory.GetFiles(targDir, "*", SearchOption.AllDirectories))
				.OrderBy(SCommon.Comp)
				.ToArray();

			List<string> dest = new List<string>();

			foreach (string path in paths)
			{
				dest.Add(SCommon.ChangeRoot(path, targDir));

				if (Directory.Exists(path))
				{
					dest.Add("\t-> Directory");
				}
				else
				{
					FileInfo info = new FileInfo(path);

					dest.Add(string.Format(
						"\t-> File {0} / {1} / {2:#,0}"
						, new SimpleDateTime(info.CreationTime)
						, new SimpleDateTime(info.LastWriteTime)
						, info.Length
						));
				}
			}
			return dest.ToArray();
		}
	}
}
