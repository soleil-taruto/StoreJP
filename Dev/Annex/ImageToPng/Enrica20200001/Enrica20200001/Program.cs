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
using Charlotte.Drawings;
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

			Main4(new ArgsReader(new string[] { @"C:\temp" }));
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

		private List<Func<Canvas, Canvas>> Filters = new List<Func<Canvas, Canvas>>();

		private void Main5(ArgsReader ar)
		{
			string strPath = SCommon.MakeFullPath(ar.NextArg());

			for (; ; )
			{
				if (ar.ArgIs("/E"))
				{
					int w = int.Parse(ar.NextArg());
					int h = int.Parse(ar.NextArg());

					if (
						w < 1 || SCommon.IMAX < w ||
						h < 1 || SCommon.IMAX < h
						)
						throw new Exception("Bad w, h");

					Filters.Add(canvas => CanvasTools.Expand(canvas, w, h));
					continue;
				}
				if (ar.ArgIs("/C"))
				{
					int w = int.Parse(ar.NextArg());
					int h = int.Parse(ar.NextArg());
					int area = int.Parse(ar.NextArg());

					if (
						w < 1 || SCommon.IMAX < w ||
						h < 1 || SCommon.IMAX < h ||
						area < 1 || 9 < area
						)
						throw new Exception("Bad w, h, area");

					Filters.Add(canvas =>
					{
						if (
							canvas.W < w ||
							canvas.H < h
							)
							throw new Exception("Bad w, h");

						int l;
						int t;

						switch ((area - 1) % 3)
						{
							case 0:
								l = 0;
								break;

							case 1:
								l = (canvas.W - w) / 2;
								break;

							case 2:
								l = canvas.W - w;
								break;

							default:
								throw null; // never
						}
						switch ((area - 1) / 3)
						{
							case 2:
								t = 0;
								break;

							case 1:
								t = (canvas.H - h) / 2;
								break;

							case 0:
								t = canvas.H - h;
								break;

							default:
								throw null; // never
						}

						Console.WriteLine(string.Format("切り取り ({0}, {1}) --> ({2}, {3}, {4}, {5})", canvas.W, canvas.H, l, t, w, h));

						return CanvasTools.GetSubImage(canvas, new I4Rect(l, t, w, h));
					});

					continue;
				}
				break;
			}
			ar.End();

			if (File.Exists(strPath))
			{
				ToPng(strPath);
			}
			else if (Directory.Exists(strPath))
			{
				foreach (string file in Directory.GetFiles(strPath, "*", SearchOption.AllDirectories))
				{
					ToPng(file);
				}
			}
			else
			{
				throw new Exception("no strPath");
			}
		}

		private void ToPng(string file)
		{
			Console.WriteLine("< " + file);

			if (IsImageFile(file))
			{
				try
				{
					using (Image image = new Bitmap(file))
					{
						string destFile = Path.Combine(SCommon.GetOutputDir(), Path.GetFileNameWithoutExtension(file) + ".png");
						destFile = SCommon.ToCreatablePath(destFile);

						Console.WriteLine("> " + destFile);

						image.Save(destFile, ImageFormat.Png);

						if (1 <= Filters.Count)
						{
							Canvas canvas = Canvas.LoadFromFile(destFile);

							foreach (Func<Canvas, Canvas> filter in Filters)
								canvas = filter(canvas);

							canvas.Save(destFile);
						}
						Console.WriteLine("done");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}

		private bool IsImageFile(string file)
		{
			string ext = Path.GetExtension(file).ToLower();

			return
				ext == ".bmp" ||
				ext == ".gif" ||
				ext == ".jpg" ||
				ext == ".jpeg" ||
				ext == ".png";
		}
	}
}
