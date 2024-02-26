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

		private static int PUID_Counter = -1;
		private static long PUID_TimeStamp = 0L;

		public static string GetPermanentUniqueID()
		{
			PUID_Counter++;

			if (0xffff < PUID_Counter)
			{
				PUID_Counter = 0;
				PUID_TimeStamp = (SimpleDateTime.FromTimeStamp(PUID_TimeStamp) + 1).ToTimeStamp();
			}
			PUID_TimeStamp = Math.Max(PUID_TimeStamp, SimpleDateTime.Now().ToTimeStamp());

			return string.Format("PUID-{0}-{1:x4}-{2}", PUID_TimeStamp, PUID_Counter, GetUUID());
		}
	}
}
