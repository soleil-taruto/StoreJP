using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.Tests
{
	public class Test0009
	{
		public void Test01()
		{
			string RES = @"

AAA
BBB
CCC
DDD
EEE
FFF

CCC-1
CCC-2
CCC-3

CCC-1-1
CCC-1-2
CCC-1-3
CCC-2-2
CCC-2-3
CCC-3-3

CCC
	CCC-1
	CCC-2
	CCC-3

CCC-1
	CCC-1-1
	CCC-1-2
	CCC-1-3
CCC-2
	CCC-2-2
	CCC-2-3
CCC-3
	CCC-3-3

DDD
	DDD-1
	DDD-2
	DDD-3

DDD-1
DDD-2
DDD-3

DDD-1-N
DDD-1-NN
DDD-1-NNN
DDD-1-NNNN
DDD-1-NNNNN

DDD-1
	DDD-1-N

DDD-1-N
	DDD-1-NN

DDD-1-NN
	DDD-1-NNN

DDD-1-NNN
	DDD-1-NNNN

DDD-1-NNNN
	DDD-1-NNNNN

";

			string[] dest = Test01_a(SCommon.TextToLines(RES));

			File.WriteAllLines(SCommon.NextOutputPath() + ".txt", dest, Encoding.UTF8);
		}

		private class Relationship_t
		{
			public string Parent;
			public List<string> Children = new List<string>();
		}

		public static string[] Test01_a(string[] lines)
		{
			Relationship_t curr = null;
			List<Relationship_t> relationships = new List<Relationship_t>();

			foreach (string line in lines.Where(v => v != ""))
			{
				if (line[0] == '\t')
				{
					curr.Children.Add(line.Trim());
				}
				else
				{
					curr = new Relationship_t()
					{
						Parent = line.Trim(),
					};

					relationships.Add(curr);
				}
			}

			{
				string[] hasChildParents = relationships
					.Where(v => v.Children.Count >= 1)
					.Select(v => v.Parent)
					.ToArray();

				relationships.RemoveAll(v => v.Children.Count == 0 && hasChildParents.Contains(v.Parent));
			}

			Test01_a2_c writer = new Test01_a2_c()
			{
				Relationships = relationships.ToArray(),
			};

			foreach (string root in relationships.Select(v => v.Parent))
			{
				writer.OutputTree(root, "");
				writer.Dest.Add("");
			}

			//File.WriteAllLines(SCommon.NextOutputPath() + ".txt", writer.Dest, Encoding.UTF8);
			return writer.Dest.ToArray();
		}

		private class Test01_a2_c
		{
			public Relationship_t[] Relationships;
			public List<string> Dest = new List<string>();

			public void OutputTree(string root, string indent)
			{
				Relationship_t curr = this.Relationships.First(v => v.Parent == root);

				this.Dest.Add(indent + root);

				indent += "\t";

				foreach (var child in curr.Children)
				{
					this.OutputTree(child, indent);
				}
			}
		}
	}
}
