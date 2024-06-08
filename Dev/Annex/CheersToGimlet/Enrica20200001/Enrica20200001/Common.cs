using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;
using Charlotte.Tools;

namespace Charlotte
{
	public static class Common
	{
		public static UniqueStringIssuer IdentifierIssuer = new P_IdentifierIssuer();

		private class P_IdentifierIssuer : UniqueStringIssuer
		{
			private const int IDENTIFIER_LENGTH = 100;

			protected override string Generate()
			{
				string identifier;

				do
				{
					identifier = "";

					while (identifier.Length < IDENTIFIER_LENGTH)
						identifier += SCommon.CRandom.ChooseOne(Consts.IDENTIFIERS);
				}
				while (identifier.Length != IDENTIFIER_LENGTH);

				return identifier;
			}
		}
	}
}
