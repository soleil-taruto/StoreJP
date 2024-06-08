using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Charlotte.Commons;
using Charlotte.Tools;

namespace Charlotte.CSSolutions
{
	public class CSFile
	{
		public string FilePath;
		public string ClassName;

		public bool FakeClassFlag = false;

		public CSFile(string filePath)
		{
			//Console.WriteLine(filePath); // test

			if (string.IsNullOrEmpty(filePath))
				throw new Exception("Bad filePath");

			if (!File.Exists(filePath))
				throw new Exception("no filePath");

			this.FilePath = filePath;
			this.ClassName = Path.GetFileNameWithoutExtension(filePath);
		}

		public string 新しい名前空間;

		public void Confuse(CSSolution sol)
		{
			string[] lines = File.ReadAllLines(this.FilePath, Encoding.UTF8);

			for (int index = 0; index < lines.Length; index++)
			{
				string line = lines[index];

				if (line.StartsWith("using Charlotte."))
					line = "";

				if (line.StartsWith("namespace "))
					line = "namespace " + this.新しい名前空間;

				lines[index] = line;
			}

			lines = sol.新しい名前空間リスト.Select(ns => "using " + ns + ";").Concat(lines).ToArray();

			File.WriteAllLines(this.FilePath, lines, Encoding.UTF8);

			// ----

			string designerFile = SCommon.ChangeExt(this.FilePath, ".Designer.cs");

			if (File.Exists(designerFile))
			{
				lines = File.ReadAllLines(designerFile, Encoding.UTF8);

				for (int index = 0; index < lines.Length; index++)
				{
					string line = lines[index];

					if (line.StartsWith("namespace "))
						line = "namespace " + this.新しい名前空間;

					lines[index] = line;
				}

				File.WriteAllLines(designerFile, lines, Encoding.UTF8);
			}
		}
	}
}
