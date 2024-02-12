using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public abstract class UniqueStringIssuer
	{
		private const int COLLISION_STRESS_LIMIT = 1000;

		protected abstract string Generate();

		private HashSet<string> IssuedStrings = SCommon.CreateSet();
		private int CollisionStress_1Of2 = 0;
		private int CollisionStress_1Of4 = 0;
		private int CollisionStress_1Of8 = 0;

		public string Issue()
		{
			for (; ; )
			{
				if (COLLISION_STRESS_LIMIT <= this.CollisionStress_1Of2)
					throw new Exception("発行可能な文字列を半分以上使い果たしました。(FATAL)");

				string str = this.Generate();

				if (string.IsNullOrEmpty(str))
					throw new Exception("不正な文字列が生成されました。");

				if (!this.IssuedStrings.Contains(str))
				{
					this.IssuedStrings.Add(str);
					this.CollisionStress_1Of2 = Math.Max(0, this.CollisionStress_1Of2 - 1);

					if (this.CollisionStress_1Of4 != -1)
						this.CollisionStress_1Of4 = Math.Max(0, this.CollisionStress_1Of4 - 1);

					if (this.CollisionStress_1Of8 != -1)
						this.CollisionStress_1Of8 = Math.Max(0, this.CollisionStress_1Of8 - 1);

					return str;
				}
				this.CollisionStress_1Of2++;

				if (this.CollisionStress_1Of4 != -1 && COLLISION_STRESS_LIMIT <= (this.CollisionStress_1Of4 += 3) / 3)
				{
					ProcMain.WriteLog("発行可能な文字列を 25 % 使い果たしました。(警告)");
					this.CollisionStress_1Of4 = -1;
				}
				if (this.CollisionStress_1Of8 != -1 && COLLISION_STRESS_LIMIT <= (this.CollisionStress_1Of8 += 7) / 7)
				{
					ProcMain.WriteLog("発行可能な文字列を 12.5 % 使い果たしました。(警告)");
					this.CollisionStress_1Of8 = -1;
				}
			}
		}
	}
}
