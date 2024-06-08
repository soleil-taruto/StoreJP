using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.CSSolutions
{
	public class CSProject
	{
		public string ProjectDir;
		public string ProjectFile;

		public CSProject(string dir, string file)
		{
			this.ProjectDir = dir;
			this.ProjectFile = file;
		}

		public void AddFakeClass(int classCount, CSSolution sol)
		{
			string[] classNames = new string[classCount];
			string[] chainMethodNames = new string[classCount];

			for (int index = 0; index < classCount; index++)
			{
				classNames[index] = Common.IdentifierIssuer.Issue();
				chainMethodNames[index] = Common.IdentifierIssuer.Issue();
			}
			for (int index = 0; index < classCount; index++)
			{
				File.WriteAllText(
					Path.Combine(this.ProjectDir, classNames[index] + ".cs"),
					string.Format(@"

namespace Charlotte
{{
	public class {0}
	{{
		public void {1}()
		{{
			new {2}().{3}();
		}}
	}}
}}

"
					, classNames[index]
					, chainMethodNames[index]
					, classNames[(index + 1) % classCount]
					, chainMethodNames[(index + 1) % classCount]
					),
					Encoding.UTF8
					);
			}
			string[] lines = this.LoadProjectFile();
			int insertPosition = this.GetSingleCompileTagLineIndexes(lines)[0];

			lines = SCommon.GetPart(lines, 0, insertPosition)
				.Concat(classNames.Select(className => this.CreateSingleCompileTagLine(className + ".cs")))
				.Concat(SCommon.GetPart(lines, insertPosition))
				.ToArray();

			File.WriteAllLines(this.ProjectFile, lines, Encoding.UTF8);
			lines = null;

			foreach (string className in classNames)
				sol.CSFiles.Add(new CSFile(Path.Combine(this.ProjectDir, className + ".cs")) { FakeClassFlag = true });
		}

		public void ShuffleCompileTagOrder()
		{
			string[] lines = this.LoadProjectFile();
			int[] swappableLineIndexes = this.GetSingleCompileTagLineIndexes(lines);

			for (int index = swappableLineIndexes.Length; 2 <= index; index--)
			{
				SCommon.Swap(
					lines
					, swappableLineIndexes[index - 1]
					, swappableLineIndexes[SCommon.CRandom.GetInt(index)]
					);
			}
			this.SaveProjectFile(lines);
		}

		public void AddFakeClass_Ph2nd(bool frontFlag, CSSolution sol)
		{
			string className = Common.IdentifierIssuer.Issue();
			string methodName_前 = Common.IdentifierIssuer.Issue();
			string methodName_後 = Common.IdentifierIssuer.Issue();

			File.WriteAllText(
				Path.Combine(this.ProjectDir, className + ".cs"),
				string.Format(@"

namespace Charlotte
{{
	public class {0}
	{{
		public void {1}()
		{{
			{2}();
		}}

		public void {3}()
		{{
			{4}();
		}}
	}}
}}

"
				, className
				, methodName_前
				, methodName_後
				, methodName_後
				, methodName_前
				),
				Encoding.UTF8
				);

			string[] lines = this.LoadProjectFile();
			int[] sCTLIndexes = this.GetSingleCompileTagLineIndexes(lines);
			int insertPosition = frontFlag ? sCTLIndexes[0] : sCTLIndexes[sCTLIndexes.Length - 1] + 1;

			lines = SCommon.GetPart(lines, 0, insertPosition)
				.Concat(new string[] { this.CreateSingleCompileTagLine(className + ".cs") })
				.Concat(SCommon.GetPart(lines, insertPosition))
				.ToArray();

			File.WriteAllLines(this.ProjectFile, lines, Encoding.UTF8);
			lines = null;

			sol.CSFiles.Add(new CSFile(Path.Combine(this.ProjectDir, className + ".cs")) { FakeClassFlag = true });
		}

		public void AddFakeClass_Ph3rd(string 新しい名前空間, CSSolution sol)
		{
			string className = Common.IdentifierIssuer.Issue();

			File.WriteAllText(
				Path.Combine(this.ProjectDir, className + ".cs"),
				string.Format(@"

namespace {0}
{{
	public class {1}
	{{
		public string {2}()
		{{
			return ""{3}"";
		}}
	}}
}}

"
				, 新しい名前空間
				, className
				, Common.IdentifierIssuer.Issue()
				, Common.IdentifierIssuer.Issue()
				),
				Encoding.UTF8
				);

			string[] lines = this.LoadProjectFile();
			int insertPosition = this.GetSingleCompileTagLineIndexes(lines)[0];

			lines = SCommon.GetPart(lines, 0, insertPosition)
				.Concat(new string[] { this.CreateSingleCompileTagLine(className + ".cs") })
				.Concat(SCommon.GetPart(lines, insertPosition))
				.ToArray();

			File.WriteAllLines(this.ProjectFile, lines, Encoding.UTF8);
			lines = null;

			sol.CSFiles.Add(new CSFile(Path.Combine(this.ProjectDir, className + ".cs")) { FakeClassFlag = true });
		}

		private string[] LoadProjectFile()
		{
			return File.ReadAllLines(this.ProjectFile, Encoding.UTF8);
		}

		private void SaveProjectFile(string[] lines)
		{
			File.WriteAllLines(this.ProjectFile, lines, Encoding.UTF8);
		}

		private int[] GetSingleCompileTagLineIndexes(string[] lines)
		{
			List<int> dest = new List<int>();

			for (int index = 0; index < lines.Length; index++)
			{
				string line = lines[index];

				if (
					line.StartsWith("    <Compile Include=\"") &&
					line.EndsWith("\" />")
					)
					dest.Add(index);
			}
			return dest.ToArray();
		}

		private string CreateSingleCompileTagLine(string filePath)
		{
			return "    <Compile Include=\"" + filePath + "\" />";
		}
	}
}
