using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Charlotte.Commons;
using Charlotte.Tests;

namespace Charlotte
{
	public class UProgram
	{
		public void Run()
		{
			try
			{
				Main2();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "UProgram / Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Main2()
		{
			if (ProcMain.DEBUG)
			{
				Main3();
			}
			else
			{
				Main4();
			}
			SCommon.OpenOutputDirIfCreated();
		}

		private void Main3()
		{
			// テスト系 -- リリース版では使用しない。
#if DEBUG
			{
				Lazy<string> logFile = new Lazy<string>(() => SCommon.NextOutputPath() + ".log");

				ProcMain.WriteLog = message =>
				{
					File.AppendAllText(logFile.Value, "[" + SimpleDateTime.Now().ToString() + "] " + message + "\r\n", Encoding.UTF8);
				};
			}

			// -- choose one --

			Main4();
			//new Test0001().Test01();
			//new Test0001().Test02();
			//new Test0001().Test03();

			// --
#endif
		}

		private void Main4()
		{
			using (HomeWin f = new HomeWin())
			{
				f.ShowDialog();
			}
		}
	}
}
