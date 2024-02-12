using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte.WebServices
{
	public class HTTPServer : SockServer
	{
		// 使用例・呼び出しテンプレート
#if false
		public void Test01()
		{
			HTTPServer hs = new HTTPServer()
			{
				PortNo = 80,
				Backlog = 300,
				ConnectMax = 100,
				Interlude = () => !Console.KeyAvailable,
				HTTPConnected = channel =>
				{
					// ★サンプル -- 要削除
					{
						// 以下は安全に表示可能な文字列であることが保証される。
						// -- FirstLine == ASCII && not-null
						// -- Method == ASCII && not-null
						// -- PathQuery == SJIS && not-null
						// -- HTTPVersion == ASCII && not-null
						// -- HeaderPairs == not-null && (全てのキーと値について ASCII && not-null)
						// ---- ASCII == [\u0020-\u007e]*
						// ---- SJIS == ToJString(, true, false, false, true)
						// 以下も保証される。
						// -- Body == not-null

						Console.WriteLine(channel.FirstLine);
						Console.WriteLine(channel.Method);
						Console.WriteLine(channel.PathQuery);
						Console.WriteLine(channel.HTTPVersion);
						Console.WriteLine(string.Join(", ", channel.HeaderPairs.Select(pair => pair[0] + "=" + pair[1])));
						Console.WriteLine(BitConverter.ToString(channel.Body.ToByteArray()));

						channel.ResStatus = 200;
						channel.ResHeaderPairs.Add(new string[] { "Content-Type", "text/plain; charset=US-ASCII" });
						channel.ResHeaderPairs.Add(new string[] { "X-Header-001", "123" });
						channel.ResHeaderPairs.Add(new string[] { "X-Header-002", "456" });
						channel.ResHeaderPairs.Add(new string[] { "X-Header-003", "789" });
						channel.ResBody = new byte[][] { Encoding.ASCII.GetBytes("Hello, Happy World!") };
						channel.ResBodyLength = -1L;
					}
				},
			};

			SockChannel.ThreadTimeoutMillis = 100;

			HTTPServer.KeepAliveTimeoutMillis = 5000;

			HTTPServerChannel.RequestTimeoutMillis = -1;
			HTTPServerChannel.ResponseTimeoutMillis = -1;
			HTTPServerChannel.FirstLineTimeoutMillis = 2000;
			HTTPServerChannel.IdleTimeoutMillis = 180000; // 3 min
			HTTPServerChannel.BodySizeMax = 512000000; // 512 MB

			SockCommon.TimeWaitMonitor.CTR_ROT_SEC = 60;
			SockCommon.TimeWaitMonitor.COUNTER_NUM = 5;
			SockCommon.TimeWaitMonitor.COUNT_LIMIT = 10000;

			hs.Run();
		}
#endif

		/// <summary>
		/// サーバーロジック
		/// 引数：
		/// -- channel: 接続チャネル
		/// </summary>
		public Action<HTTPServerChannel> HTTPConnected = channel => { };

		/// <summary>
		/// キープアライブのタイムアウト_ミリ秒
		/// -1 == INFINITE
		/// </summary>
		public static int KeepAliveTimeoutMillis = 5000;

		// <---- init if needed

		public HTTPServer()
		{
			PortNo = 80;
		}

		protected override IEnumerable<int> E_Connected(SockChannel channel)
		{
			DateTime startedTime = DateTime.Now;

			for (; ; )
			{
				HTTPServerChannel hsChannel = new HTTPServerChannel();
				int retval = -1;

				hsChannel.Channel = channel;

				foreach (int size in hsChannel.RecvRequest())
				{
					if (size == 0)
						throw null; // never

					if (size < 0)
					{
						yield return retval;
						retval = -1;
					}
					else
					{
						retval = 1;
					}
				}

				SockCommon.NB("svlg", () =>
				{
					HTTPConnected(hsChannel);
					return -1; // dummy
				});

				if (KeepAliveTimeoutMillis != -1 && KeepAliveTimeoutMillis < (DateTime.Now - startedTime).TotalMilliseconds)
				{
					hsChannel.KeepAlive = false;
				}

				foreach (int size in hsChannel.SendResponse())
				{
					if (size == 0)
						throw null; // never

					if (size < 0)
					{
						yield return retval;
						retval = -1;
					}
					else
					{
						retval = 1;
					}
				}

				if (!hsChannel.KeepAlive)
					break;
			}
		}
	}
}
