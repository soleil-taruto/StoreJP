﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charlotte.Commons;

namespace Charlotte.Tools
{
	public class CtrCipher : IDisposable
	{
		private static AESCipher SharedTransformer = null;

		public static CtrCipher CreateTemporary()
		{
			if (SharedTransformer == null)
				SharedTransformer = new AESCipher(SCommon.CRandom.GetBytes(32));

			return new CtrCipher(SharedTransformer, SCommon.CRandom.GetBytes(16));
		}

		public static CtrCipher CreateTemporaryHeavy()
		{
			return new CtrCipher(SCommon.CRandom.GetBytes(32), SCommon.CRandom.GetBytes(16));
		}

		private AESCipher Transformer;
		private bool TransformerInternal;
		private byte[] InitializationVector;
		private byte[] Counter = new byte[16];
		private byte[] Buffer = new byte[16];
		private int Index;

		private CtrCipher(AESCipher transformer, byte[] iv)
		{
			if (
				transformer == null ||
				iv == null ||
				iv.Length != 16
				)
				throw new ArgumentException();

			this.Transformer = transformer;
			this.TransformerInternal = false;
			this.InitializationVector = iv;
			this.Reset();
		}

		public CtrCipher(byte[] rawKey, byte[] iv)
		{
			if (
				rawKey == null ||
				!IsAllowedRawKeyLength(rawKey.Length) ||
				iv == null ||
				iv.Length != 16
				)
				throw new ArgumentException();

			this.Transformer = new AESCipher(rawKey);
			this.TransformerInternal = true;
			this.InitializationVector = iv;
			this.Reset();
		}

		private static bool IsAllowedRawKeyLength(int length)
		{
			return
				length == 16 ||
				length == 24 ||
				length == 32;
		}

		public void Reset()
		{
			Array.Copy(this.InitializationVector, this.Counter, 16);
			this.Index = 16;
		}

		public byte Next()
		{
			if (this.Index == 16)
			{
				this.Transformer.EncryptBlock(this.Counter, this.Buffer);
				this.Increment();
				this.Index = 0;
			}
			return this.Buffer[this.Index++];
		}

		private void Increment()
		{
			for (int index = 0; index < 16; index++)
			{
				if (this.Counter[index] < 255)
				{
					this.Counter[index]++;
					break;
				}
				this.Counter[index] = 0;
			}
		}

		public void Mask(byte[] data, int offset = 0)
		{
			this.Mask(data, offset, data.Length - offset);
		}

		public void Mask(byte[] data, int offset, int count)
		{
			this.Mask(data, offset, data, offset, count);
		}

		public void Mask(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
		{
			for (int index = 0; index < count; index++)
			{
				dest[destOffset + index] = (byte)(src[srcOffset + index] ^ this.Next());
			}
		}

		public void Dispose()
		{
			if (this.Transformer != null)
			{
				if (this.TransformerInternal)
					this.Transformer.Dispose();

				this.Transformer = null;
			}
		}
	}
}
