using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Windows.Forms;
using Microsoft.Win32;
using Charlotte.Commons;

namespace Charlotte
{
	public static class GUIProcMain
	{
		public static void GUIMain(Func<Form> getMainForm)
		{
			ProcMain.WriteLog = message => { };

			Application.ThreadException += new ThreadExceptionEventHandler((sender, e) => ErrorTermination(e.Exception));
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) => ErrorTermination(e.ExceptionObject));
			SystemEvents.SessionEnding += new SessionEndingEventHandler((sender, e) => ProgramTermination());

			KeepSingleInstance(() =>
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(getMainForm());
			});
		}

		private static void ErrorTermination(object ex)
		{
			MessageBox.Show(
				"" + ex,
				Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
				);

			ProgramTermination();
		}

		private static void ProgramTermination()
		{
			Environment.Exit(1);
		}

		private static void KeepSingleInstance(Action routine)
		{
			// HACK: 同じアプリケーションでもビルドしなおすと(バージョンが違うと)排他制御が効かなくなる。

			string selfFileHash = GetSelfFileHash();

			Mutex procMutex = new Mutex(
				false,
				"Silvia20200001_PROC_MUTEX_{62f72aca-dc8c-432e-b00d-e589dc2bf9fa}_" + selfFileHash
				);

			if (procMutex.WaitOne(0))
			{
				bool createdNew;
				MutexSecurity security = new MutexSecurity();
				security.AddAccessRule(
					new MutexAccessRule(
						new SecurityIdentifier(
							WellKnownSidType.WorldSid,
							null
							),
						MutexRights.FullControl,
						AccessControlType.Allow
						)
					);
				Mutex globalProcMutex = new Mutex(
					false,
					@"Global\Silvia20200001_GLOBAL_PROC_MUTEX_{ffdbdfa1-6ba8-4ec5-899c-9b361bbb6a15}_" + selfFileHash,
					out createdNew,
					security
					);

				bool globalLockFailed = false;

				if (globalProcMutex.WaitOne(0))
				{
					routine();

					globalProcMutex.ReleaseMutex();
				}
				else
				{
					globalLockFailed = true;
				}
				globalProcMutex.Close();
				globalProcMutex.Dispose();
				globalProcMutex = null;

				if (globalLockFailed)
				{
					// memo: ローカル側ロック(procMutex)解除前に表示すること。
					// -- プロセスを同時に複数起動したとき、このダイアログを複数表示させないため。
					//
					MessageBox.Show(
						"This program is running in another logon session !",
						Path.GetFileNameWithoutExtension(ProcMain.SelfFile) + " / Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
						);
				}
				procMutex.ReleaseMutex();
			}
			procMutex.Close();
			procMutex.Dispose();
			procMutex = null;
		}

		private static string GetSelfFileHash()
		{
			bool createdNew;
			MutexSecurity security = new MutexSecurity();
			security.AddAccessRule(
				new MutexAccessRule(
					new SecurityIdentifier(
						WellKnownSidType.WorldSid,
						null
						),
					MutexRights.FullControl,
					AccessControlType.Allow
					)
				);
			Mutex globalMutex = new Mutex(
				false,
				@"Global\Silvia20200001_GLOBAL_MUTEX_{c8d945d1-5d7e-405c-bedd-f3668960bdab}",
				out createdNew,
				security
				);

			globalMutex.WaitOne();

			string hashStoredFile = ProcMain.SelfFile + "_sha512.dat";
			string hash;

			if (File.Exists(hashStoredFile))
			{
				hash = File.ReadAllText(hashStoredFile, Encoding.ASCII).Trim();

				if (!Regex.IsMatch(hash, "^[0-9a-f]{128}$"))
					throw new Exception("Hash file is corrupt !");
			}
			else
			{
				using (SHA512 sha512 = SHA512.Create())
				using (FileStream reader = new FileStream(ProcMain.SelfFile, FileMode.Open, FileAccess.Read))
				{
					hash = BitConverter.ToString(sha512.ComputeHash(reader)).Replace("-", "").ToLower();
				}

				File.WriteAllText(hashStoredFile, hash, Encoding.ASCII);
			}

			globalMutex.ReleaseMutex();
			globalMutex.Close();
			globalMutex.Dispose();
			globalMutex = null;

			return hash;
		}
	}
}
