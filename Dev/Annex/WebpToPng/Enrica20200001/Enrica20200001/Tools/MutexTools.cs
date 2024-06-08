using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class MutexTools
	{
		public static IDisposable Section(Mutex mutex)
		{
			mutex.WaitOne();
			return SCommon.GetAnonyDisposable(() => mutex.ReleaseMutex());
		}
	}
}
