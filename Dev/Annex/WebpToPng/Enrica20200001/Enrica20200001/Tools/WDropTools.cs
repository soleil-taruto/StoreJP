using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class WDropTools
	{
		private static string WDropExeFile = @"C:\app\WDrop\WDrop.exe";

		public static string Run()
		{
			if (!File.Exists(WDropExeFile))
				throw new Exception("no WDropExeFile");

			string mutexName = UUIDTools.GetPermanentUniqueID();

			using (Mutex mutex = new Mutex(false, mutexName))
			using (MutexTools.Section(mutex))
			using (WorkingDir wd = new WorkingDir())
			{
				string wFile = wd.MakePath();

				SCommon.Batch(new string[]
				{
					string.Format("{0} \"{1}\" {2} {3} {4}", WDropExeFile, ProcMain.SelfFile, Process.GetCurrentProcess().Id, mutexName, wFile),
				});

				if (!File.Exists(wFile))
					return null;

				string inputtedPath = File.ReadAllText(wFile).Trim();

				if (string.IsNullOrEmpty(inputtedPath))
					return null;

				return inputtedPath;
			}
		}
	}
}
