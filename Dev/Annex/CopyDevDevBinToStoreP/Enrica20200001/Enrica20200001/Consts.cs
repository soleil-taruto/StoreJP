using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charlotte
{
	public static class Consts
	{
		/// <summary>
		/// 入力ルートディレクトリのフォーマット
		/// -- {0} == devDirName
		/// </summary>
		public const string R_ROOT_DIR_FORMAT = @"C:\{0}";

		/// <summary>
		/// 出力ルートディレクトリのフォーマット
		/// -- {0} == devDirName
		/// -- {1} == alpha
		/// </summary>
		public const string W_ROOT_DIR_FORMAT = @"C:\home\GitHub\Store{1}\{0}";

		/// <summary>
		/// 出力ディレクトリの期限切れまでの秒数
		/// 期限切れになると再作成する。
		/// </summary>
		public const long CR_W_DIR_EXPIRE_SEC = 86400 * 30;
	}
}
