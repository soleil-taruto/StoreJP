using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Charlotte.Commons;

namespace Charlotte
{
	public static class Common
	{
		public static byte[] Replace(byte[] srcData, byte[] srcPtn, byte[] destPtn)
		{
			using (MemoryStream destData = new MemoryStream())
			{
				for (int index = 0; index < srcData.Length; )
				{
					if (index + srcPtn.Length <= srcData.Length && IsSamePart(srcData, index, srcPtn, 0, srcPtn.Length))
					{
						SCommon.Write(destData, destPtn);
						index += srcPtn.Length;
					}
					else
					{
						destData.WriteByte(srcData[index]);
						index++;
					}
				}
				return destData.ToArray();
			}
		}

		private static bool IsSamePart(byte[] data1, int offset1, byte[] data2, int offset2, int size)
		{
			for (int index = 0; index < size; index++)
				if (data1[offset1 + index] != data2[offset2 + index])
					return false;

			return true;
		}
	}
}
