using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public static class UUIDTools
	{
		public static string GetUUID()
		{
			return Guid.NewGuid().ToString("B");
		}

		private static UInt16 PUID_Counter = 0;

		public static string GetPermanentUniqueID()
		{
			return string.Format("PUID-{0}-{1:x4}-{2}", SimpleDateTime.Now().ToTimeStamp(), PUID_Counter++, GetUUID());
		}
	}
}
