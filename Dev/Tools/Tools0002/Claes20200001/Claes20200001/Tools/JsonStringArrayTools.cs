using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class JsonStringArrayTools
	{
		public static string[] ToStringArray(byte[] bText)
		{
			if (bText == null)
				throw new Exception("Bad bText");

			string text = SCommon.UTF8Conv.ToJString(bText); // ここで安全な文字列を担保する。(1/2)
			JsonTools.Node root = JsonTools.Load(text);

			if (root.Array == null)
				throw new Exception("JSON is not array");

			string[] lines = new string[root.Array.Count];
			int w = 0;

			foreach (JsonTools.Node element in root.Array)
			{
				if (element.StringValue == null)
					throw new Exception("JSON element is not string");

				string line = element.StringValue;
				line = SCommon.ToJString(line, true, false, false, true).Trim(); // ここで安全な文字列を担保する。(2/2)
				lines[w++] = line;
			}
			return lines;
		}

		public static string ToJson(IList<string> lines)
		{
			if (
				lines == null ||
				lines.Any(line => line == null)
				)
				throw new Exception("Bad lines");

			JsonTools.Node root = new JsonTools.Node()
			{
				Array = new List<JsonTools.Node>(),
			};

			foreach (string line in lines)
			{
				JsonTools.Node element = new JsonTools.Node()
				{
					StringValue = line,
				};

				root.Array.Add(element);
			}
			return JsonTools.GetString(root);
		}
	}
}
