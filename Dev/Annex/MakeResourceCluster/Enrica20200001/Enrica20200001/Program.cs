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

			//Main4(new ArgsReader(new string[] { "/R", @"C:\Dev\Dough\Game\Gattonero20200001\Gattonero20200001", @"C:\home\Resource", @"C:\temp\Cluster.dat", @"C:\temp\Authors.txt" }));
			Main4(new ArgsReader(new string[] { "/R", @"C:\Dev\Program\Pochittosan\Gattonero20200001\Gattonero20200001", @"C:\home\Resource", @"C:\temp\Cluster.dat", @"C:\temp\Authors.txt" }));
			//Main4(new ArgsReader(new string[] { "/S", @"C:\Dev\Dough\Game\Storage", @"C:\temp\Cluster.dat" }));
			//Main4(new ArgsReader(new string[] { "/S", @"C:\Dev\Program\Pochittosan\Storage", @"C:\temp\Cluster.dat" }));

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
			if (ar.ArgIs("/R"))
			{
				string sourceDir = SCommon.MakeFullPath(ar.NextArg());
				string rRootDir = SCommon.MakeFullPath(ar.NextArg());
				string clusterFile = SCommon.MakeFullPath(ar.NextArg());
				string authorsFile = SCommon.MakeFullPath(ar.NextArg());

				ar.End();

				MakeResourceCluster(sourceDir, rRootDir, clusterFile, authorsFile);
				return;
			}
			if (ar.ArgIs("/S"))
			{
				string storageDir = SCommon.MakeFullPath(ar.NextArg());
				string clusterFile = SCommon.MakeFullPath(ar.NextArg());

				ar.End();

				MakeStorageCluster(storageDir, clusterFile);
				return;
			}
			throw new Exception("不明なコマンド引数");
		}

		private void MakeResourceCluster(string sourceDir, string rRootDir, string clusterFile, string authorsFile)
		{
			ProcMain.WriteLog("< " + sourceDir);
			ProcMain.WriteLog("R " + rRootDir);
			ProcMain.WriteLog("> " + clusterFile);
			ProcMain.WriteLog("A " + authorsFile);

			if (!Directory.Exists(sourceDir))
				throw new Exception("no sourceDir");

			if (!Directory.Exists(rRootDir))
				throw new Exception("no rRootDir");

			if (Directory.Exists(clusterFile))
				throw new Exception("Bad clusterFile");

			if (Directory.Exists(authorsFile))
				throw new Exception("Bad authorsFile");

			List<string> dest = new List<string>();

			CollectResourceFilesFromSourceFile(dest, rRootDir, sourceDir, "Fonts.cs");
			CollectResourceFilesFromSourceFile(dest, rRootDir, sourceDir, "Musics.cs");
			CollectResourceFilesFromSourceFile(dest, rRootDir, sourceDir, "Pictures.cs");
			CollectResourceFilesFromSourceFile(dest, rRootDir, sourceDir, "SoundEffects.cs");

			MakeResourceCluster_Main(
				rRootDir,
				dest.Select(v => Path.Combine(rRootDir, v)).ToArray(),
				clusterFile
				);

			MakeAuthorsFile(
				rRootDir,
				dest.Select(v => Path.Combine(rRootDir, v)).ToArray(),
				authorsFile
				);

			ProcMain.WriteLog("done!");
		}

		private void CollectResourceFilesFromSourceFile(List<string> dest, string rRootDir, string sourceDir, string sourceLocalName)
		{
			string sourceFile = Path.Combine(sourceDir, sourceLocalName);

			if (!File.Exists(sourceFile))
				throw new Exception("no " + sourceLocalName);

			string source = File.ReadAllText(sourceFile, Encoding.UTF8);

			for (; ; )
			{
				string[] encl = SCommon.ParseEnclosed(source, "@\"", "\"");

				if (encl == null)
					break;

				string resPath = encl[2];

				if (SCommon.IsFairRelPath(resPath, -1))
				{
					string filePath = Path.Combine(rRootDir, resPath);

					if (File.Exists(filePath))
						dest.Add(resPath);
				}
				source = encl[4];
			}
		}

		private void MakeStorageCluster(string storageDir, string clusterFile)
		{
			ProcMain.WriteLog("< " + storageDir);
			ProcMain.WriteLog("> " + clusterFile);

			if (!Directory.Exists(storageDir))
				throw new Exception("no storageDir");

			if (Directory.Exists(clusterFile))
				throw new Exception("Bad clusterFile");

			string[] files = Directory.GetFiles(storageDir, "*", SearchOption.AllDirectories);

			// 半角アンダースコアで始まるファイルとディレクトリを除外する。
			{
				files = files
					.Where(file => !SCommon.Tokenize(SCommon.ChangeRoot(file, storageDir), "\\").Any(pTkn => pTkn[0] == '_'))
					.ToArray();
			}

			MakeResourceCluster_Main(
				storageDir,
				files,
				clusterFile
				);

			ProcMain.WriteLog("done!");
		}

		private void MakeResourceCluster_Main(string rRootDir, string[] rFiles, string clusterFile)
		{
			rFiles = rFiles.DistinctOrderBy(SCommon.CompIgnoreCase).ToArray();

			using (FileStream writer = new FileStream(clusterFile, FileMode.Create, FileAccess.Write))
			{
				foreach (string rFile in rFiles)
				{
					string resPath = SCommon.ChangeRoot(rFile, rRootDir);
					byte[] data = File.ReadAllBytes(rFile);
					int originalDataSize = data.Length;

					ProcMain.WriteLog("+ " + resPath);
					ProcMain.WriteLog("S " + originalDataSize);

					data = SCommon.Compress(data);
					ShuffleT023109(data);

					SCommon.WritePartString(writer, resPath);
					SCommon.WritePartInt(writer, originalDataSize);
					SCommon.WritePartInt(writer, data.Length);
					SCommon.Write(writer, data);

					ProcMain.WriteLog("done");
				}
			}
		}

		private static void ShuffleT023109(byte[] data)
		{
			int l = 0;
			int r = data.Length - 2;
			int rr = Math.Max(3, data.Length / 109);

			while (l < r)
			{
				SCommon.Swap(data, l, r);

				l++;
				r -= rr;
			}
		}

		private const string SOURCE_OF_RESOURCE_LOCAL_NAME = "_source.txt";

		private void MakeAuthorsFile(string rRootDir, string[] rFiles, string authorsFile)
		{
			HashSet<string> srcOfResFiles = SCommon.CreateSetIgnoreCase();

			foreach (string rFile in rFiles)
			{
				string[] pTkns = SCommon.Tokenize(SCommon.ChangeRoot(rFile, rRootDir), "\\");

				for (int pTknSpn = 1; pTknSpn + 1 <= pTkns.Length; pTknSpn++) // rRootDirの直下 ～ rFileと同じ場所
				{
					string srcOfResFile = Path.Combine(new string[] { rRootDir }
						.Concat(pTkns.Take(pTknSpn))
						.Concat(new string[] { SOURCE_OF_RESOURCE_LOCAL_NAME })
						.ToArray());

					srcOfResFiles.Add(srcOfResFile);
				}
			}

			using (StreamWriter writer = new StreamWriter(authorsFile, false, SCommon.ENCODING_SJIS))
			{
				writer.WriteLine("==== RESOURCE AUTHORS ====");
				writer.WriteLine("-- 順不同");
				writer.WriteLine("-- 敬称略");
				writer.WriteLine();

				foreach (string srcOfResFile in srcOfResFiles.OrderBy(SCommon.CompIgnoreCase))
				{
					ProcMain.WriteLog("A " + srcOfResFile);

					if (File.Exists(srcOfResFile))
					{
						ProcMain.WriteLog("A +");

						using (StreamReader reader = new StreamReader(srcOfResFile, SCommon.ENCODING_SJIS))
						{
							for (int lineIndex = 0; ; lineIndex++)
							{
								string line = reader.ReadLine();

								if (line == null)
									break;

								writer.WriteLine(new string('　', lineIndex) + line);
							}
							writer.WriteLine();
						}
					}
				}
			}
		}
	}
}
