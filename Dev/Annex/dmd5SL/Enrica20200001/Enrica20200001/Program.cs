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
			Directory.SetCurrentDirectory(@"C:\Dev");

			// -- choose one --

			Main4(new ArgsReader(new string[] { "Commons" }));
			//Main4(new ArgsReader(new string[] { "Drawings" }));
			//Main4(new ArgsReader(new string[] { "GameCommons" }));

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
			string targetLocalDirName = ar.NextArg();
			ar.End();

			string rootDir = Directory.GetCurrentDirectory();

			Queue<string> q = new Queue<string>();
			q.Enqueue(rootDir);

			while (1 <= q.Count)
			{
				foreach (string dir in Directory.GetDirectories(q.Dequeue()))
				{
					string localDirName = Path.GetFileName(dir);

					if (targetLocalDirName.EqualsIgnoreCase(localDirName))
					{
						using (WorkingDir wd = new WorkingDir())
						{
							string outFile = wd.MakePath();

							SCommon.Batch(new string[]
							{
								"C:\\Factory\\Tools\\dmd5.exe /S > " + outFile 
							},
							dir
							);

							string hash = File.ReadAllText(outFile).Trim();

							Console.WriteLine(hash + " " + dir);
						}
					}
					else
					{
						q.Enqueue(dir);
					}
				}
			}
		}
	}
}
