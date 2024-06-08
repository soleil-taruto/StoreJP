using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Charlotte.Commons;
using Charlotte.Tools;

namespace Charlotte
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcMain.CUIMain(new Program().Main2);
		}

		private void Main2(ArgsReader ar)
		{
			if (ProcMain.DEBUG)
			{
				Main3();
			}
			else
			{
				Main4(ar);
			}
			SCommon.OpenOutputDirIfCreated();
		}

		private void Main3()
		{
			// テスト系 -- リリース版では使用しない。
#if DEBUG
			// -- choose one --

			Main4(new ArgsReader(new string[] { }));
			//Main4(new ArgsReader(new string[] { }));
			//Main4(new ArgsReader(new string[] { }));

			// --
#endif
			SCommon.Pause();
		}

		private void Main4(ArgsReader ar)
		{
			try
			{
				Main5(ar);
			}
			catch (Exception ex)
			{
				ProcMain.WriteLog(ex);

				MessageBox.Show("" + ex, Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

				//Console.WriteLine("Press ENTER key. (エラーによりプログラムを終了します)");
				//Console.ReadLine();
			}
		}

		private void Main5(ArgsReader ar)
		{
			string[] exts = new string[0];
			bool ignoreCase = false;

			for (; ; )
			{
				if (ar.ArgIs("/E"))
				{
					exts = SCommon.Tokenize(ar.NextArg(), ".")
						.Select(v => v.Trim())
						.Where(v => v != "")
						.Select(v => "." + v)
						.ToArray();

					continue;
				}
				if (ar.ArgIs("/I"))
				{
					ignoreCase = true;
					continue;
				}
				break;
			}
			string searchPtn = SCommon.ToJString(ar.NextArg(), true, false, false, true);

			if (searchPtn == "")
				throw new Exception("no searchPtn");

			byte[][][] bSearchPTbl_01 = GetBSearchPTbl(searchPtn, ignoreCase, Encoding.UTF8);
			byte[][][] bSearchPTbl_02 = GetBSearchPTbl(searchPtn, ignoreCase, Encoding.Unicode);
			byte[][][] bSearchPTbl_03 = GetBSearchPTbl(searchPtn, ignoreCase, Encoding.BigEndianUnicode);
			byte[][][] bSearchPTbl_04 = GetBSearchPTbl(searchPtn, ignoreCase, SCommon.ENCODING_SJIS);

			string[] files = Directory.GetFiles(".", "*", SearchOption.AllDirectories)
				.Select(v => SCommon.MakeFullPath(v))
				.Where(v => exts.Length == 0 || exts.Any(w => w.EqualsIgnoreCase(Path.GetExtension(v))))
				.OrderBy(SCommon.CompIgnoreCase)
				.ToArray();

			foreach (string file in files)
			{
				try
				{
					using (BlockBufferedFileReader reader = new BlockBufferedFileReader(file))
					{
						if (
							Search(reader, bSearchPTbl_01) ||
							Search(reader, bSearchPTbl_02) ||
							Search(reader, bSearchPTbl_03) ||
							Search(reader, bSearchPTbl_04)
							)
							Console.WriteLine(file);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(file + " <READ-ERROR> " + ex);
				}
			}
		}

		private byte[][][] GetBSearchPTbl(string searchPtn, bool ignoreCase, Encoding encoding)
		{
			List<byte[][]> bSearchPTbl = new List<byte[][]>();

			if (ignoreCase)
			{
				foreach (char chr in searchPtn)
				{
					string sChr = new string(new char[] { chr });
					string lwr = sChr.ToLower();
					string upr = sChr.ToUpper();

					List<byte[]> buff = new List<byte[]>();

					buff.Add(encoding.GetBytes(lwr));

					if (lwr != upr)
						buff.Add(encoding.GetBytes(upr));

					bSearchPTbl.Add(buff.ToArray());
				}
			}
			else
			{
				bSearchPTbl.Add(new byte[][] { encoding.GetBytes(searchPtn) });
			}
			return bSearchPTbl.ToArray();
		}

		private bool Search(BlockBufferedFileReader reader, byte[][][] bSearchPTbl)
		{
			Search_Reader = reader;
			Search_BSearchPTbl = bSearchPTbl;
			try
			{
				return Search_Main();
			}
			finally
			{
				Search_Reader = null;
				Search_BSearchPTbl = null;
			}
		}

		private BlockBufferedFileReader Search_Reader;
		private byte[][][] Search_BSearchPTbl;

		private bool Search_Main()
		{
			for (long offset = 0; offset < Search_Reader.Length; offset++)
				if (Search_F01(offset, 0))
					return true;

			return false;
		}

		private bool Search_F01(long offset, int bSPTIndex)
		{
			if (Search_BSearchPTbl.Length <= bSPTIndex)
				return true;

			foreach (byte[] bSearchPtn in Search_BSearchPTbl[bSPTIndex])
			{
				if (
					Search_F02(offset, bSearchPtn) &&
					Search_F01(offset + bSearchPtn.Length, bSPTIndex + 1)
					)
					return true;
			}
			return false;
		}

		private bool Search_F02(long offset, byte[] bSearchPtn)
		{
			if (Search_Reader.Length < offset + bSearchPtn.Length)
				return false;

			for (int index = 0; index < bSearchPtn.Length; index++)
				if (Search_Reader[offset + index] != bSearchPtn[index])
					return false;

			return true;
		}
	}
}
