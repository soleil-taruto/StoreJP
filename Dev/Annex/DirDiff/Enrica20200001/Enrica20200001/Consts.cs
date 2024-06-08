using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Charlotte
{
	public static class Consts
	{
		public static readonly string FIXED_TEMP_DIR = @"C:\temp\DirDiff.tmp";
		public static readonly string FILE_FOR_COMP_01 = Path.Combine(FIXED_TEMP_DIR, "compare1.txt");
		public static readonly string FILE_FOR_COMP_02 = Path.Combine(FIXED_TEMP_DIR, "compare2.txt");
		public static readonly string COMP_STDOUT_FILE = Path.Combine(FIXED_TEMP_DIR, "stdout.txt");
		public static readonly long COMPARE_FILE_SIZE_MAX = 30000000; // 30 MB
	}
}
