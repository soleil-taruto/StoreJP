using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.CSSolutions
{
	public class CSSolution
	{
		public string Dir;
		public string Name;
		public string MSVSSolutionFile;
		public string ProjectDir;
		public string ProjectFile;
		public string OutputFile;
		public CSProject Project;
		public List<CSFile> CSFiles = new List<CSFile>();

		public CSSolution(string dir, string name)
		{
			if (string.IsNullOrEmpty(dir))
				throw new Exception("Bad dir");

			if (!Directory.Exists(dir))
				throw new Exception("no dir");

			if (string.IsNullOrEmpty(name))
				throw new Exception("Bad name");

			this.Dir = dir;
			this.Name = name;
			this.MSVSSolutionFile = Path.Combine(dir, name + ".sln");
			this.ProjectDir = Path.Combine(dir, name);
			this.ProjectFile = Path.Combine(dir, name, name + ".csproj");
			this.OutputFile = Path.Combine(dir, name, "bin", "Release", name + ".exe");

			if (!File.Exists(this.MSVSSolutionFile))
				throw new Exception("no MSVSSolutionFile");

			if (!Directory.Exists(this.ProjectDir))
				throw new Exception("no ProjectDir");

			if (!File.Exists(this.ProjectFile))
				throw new Exception("no ProjectFile");

			this.Project = new CSProject(this.ProjectDir, this.ProjectFile);

			this.Clean(); // ごみファイルをソースファイル(*.cs)として拾わないように先ずクリーンする必要がある。

			foreach (string file in Directory.GetFiles(this.ProjectDir, "*.cs", SearchOption.AllDirectories).OrderBy(SCommon.CompIgnoreCase))
			{
				if (file.ContainsIgnoreCase("\\Properties\\")) // 除外
					continue;

				if (file.EndsWithIgnoreCase(".Designer.cs")) // 除外
					continue;

				this.CSFiles.Add(new CSFile(file));
			}

			// ★名前空間をめちゃくちゃに置き換えるので、クラス名の重複は不可
			//
			if (SCommon.HasSame(this.CSFiles, (a, b) => SCommon.EqualsIgnoreCase(
				a.ClassName,
				b.ClassName
				)))
				throw new Exception("想定外：クラス名の重複");
		}

		public void Clean()
		{
			ProcMain.WriteLog("Clean-ST");
			SCommon.CreateDir(SCommon.ToParentPath(this.OutputFile));
			File.WriteAllBytes(this.OutputFile, SCommon.EMPTY_BYTES);
			SCommon.Batch(
				new string[]
				{
					"CALL C:\\Factory\\SetEnv.bat",
					"CALL qq",
				}
				, this.Dir
				, SCommon.StartProcessWindowStyle_e.MINIMIZED
				);

			if (File.Exists(this.OutputFile))
				throw new Exception("クリーンに失敗しました。");

			ProcMain.WriteLog("Clean-ED");
		}

		public string[] 新しい名前空間リスト;

		public void Confuse()
		{
			this.Project.AddFakeClass(this.CSFiles.Count + this.CSFiles.Count / 3 + 7, this);
			this.Project.ShuffleCompileTagOrder();

			// コンパイルタグの先頭3つ・終端3つはフェイククラスにする。
			{
				this.Project.AddFakeClass_Ph2nd(false, this); // 1
				this.Project.AddFakeClass_Ph2nd(false, this); // 2
				this.Project.AddFakeClass_Ph2nd(false, this); // 3

				this.Project.AddFakeClass_Ph2nd(true, this); // 1
				this.Project.AddFakeClass_Ph2nd(true, this); // 2
				this.Project.AddFakeClass_Ph2nd(true, this); // 3
			}

			// 新しい名前空間の割り当て
			{
				foreach (CSFile file in this.CSFiles)
				{
					file.新しい名前空間 = string.Format("Charlotte.Gattonero.CheersToGimlet.{0}"
						, Common.IdentifierIssuer.Issue()
						);
				}
				this.新しい名前空間リスト = this.CSFiles.Select(file => file.新しい名前空間).ToArray();
			}

			// 新しい名前空間リストの先頭3つ・終端3つはフェイククラスにする。
			{
				int[] フェイククラスにする位置リスト = new int[]
				{
					0, // 1
					1, // 2
					2, // 3

					this.CSFiles.Count - 3, // 1
					this.CSFiles.Count - 2, // 2
					this.CSFiles.Count - 1, // 3
				};

				foreach (int p in フェイククラスにする位置リスト)
				{
					this.CSFiles.Sort((a, b) => SCommon.Comp(a.新しい名前空間, b.新しい名前空間));

					if (!this.CSFiles[p].FakeClassFlag)
					{
						int q;

						do
						{
							q = SCommon.CRandom.GetInt(this.CSFiles.Count);
						}
						while (フェイククラスにする位置リスト.Contains(q) || !this.CSFiles[q].FakeClassFlag);

						SCommon.Swap(
							ref this.CSFiles[p].新しい名前空間,
							ref this.CSFiles[q].新しい名前空間
							);
					}
				}
			}

			Array.Sort(this.新しい名前空間リスト, SCommon.Comp);

			foreach (CSFile file in this.CSFiles)
			{
				file.Confuse(this);
			}

			this.Project.AddFakeClass_Ph3rd("Charlotte", this);
			this.Project.AddFakeClass_Ph3rd("Charlotte.Gattonero", this);
			this.Project.AddFakeClass_Ph3rd("Charlotte.Gattonero.CheersToGimlet", this);
		}

		public void Build()
		{
			ProcMain.WriteLog("Build-ST");
			SCommon.DeletePath(this.OutputFile);
			SCommon.Batch(
				new string[]
				{
					"CALL C:\\Factory\\SetEnv.bat",
					"cx **",
				}
				, this.Dir
				, SCommon.StartProcessWindowStyle_e.MINIMIZED
				);

			if (!File.Exists(this.OutputFile))
				throw new Exception("ビルドに失敗しました。");

			ProcMain.WriteLog("Build-ED");
		}
	}
}
