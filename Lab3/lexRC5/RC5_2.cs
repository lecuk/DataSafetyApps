using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexRC5
{
	public class RC5_2
	{
		const int W = 64; // word size

		public int R { get; set; }

		const UInt64 PW = 0xB7E151628AED2A6B;
		const UInt64 QW = 0x9E3779B97F4A7C15;

		UInt64[] L;
		UInt64[] S;
		int t;
		int b;
		int u;
		int c;

		public RC5_2(byte[] key)
		{
			UInt64 x, y;
			int i, j, n;

			u = 8;
			b = key.Length;
			c = ((b % u) > 0)
				? (b / u + 1)
				: (b / u);
			L = new UInt64[c];

			for (i = b - 1; i >= 0; i--)
			{
				L[i / u] = ROL(L[i / u], 8) + key[i];
			}
			
			t = 2 * (R + 1);
			S = new UInt64[t];
			S[0] = PW;
			for (i = 1; i < t; i++)
			{
				S[i] = S[i - 1] + QW;
			}
			
			x = y = 0;
			i = j = 0;
			n = 3 * Math.Max(t, c);

			for (int k = 0; k < n; k++)
			{
				x = S[i] = ROL((S[i] + x + y), 3);
				y = L[j] = ROL((L[j] + x + y), (int)(x + y));
				i = (i + 1) % t;
				j = (j + 1) % c;
			}
		}
		
		private UInt64 ROL(UInt64 a, int offset)
		{
			UInt64 r1, r2;
			r1 = a << offset;
			r2 = a >> (W - offset);
			return (r1 | r2);
		}
		
		private UInt64 ROR(UInt64 a, int offset)
		{
			UInt64 r1, r2;
			r1 = a >> offset;
			r2 = a << (W - offset);
			return (r1 | r2);
		}
		
		private static UInt64 BytesToUInt64(byte[] b, int p)
		{
			UInt64 r = 0;
			for (int i = p + 7; i > p; i--)
			{
				r |= (UInt64)b[i];
				r <<= 8;
			}
			r |= (UInt64)b[p];
			return r;
		}
		
		private static void UInt64ToBytes(UInt64 a, byte[] b, int p)
		{
			for (int i = 0; i < 7; i++)
			{
				b[p + i] = (byte)(a & 0xFF);
				a >>= 8;
			}
			b[p + 7] = (byte)(a & 0xFF);
		}
		
		public void Cipher(byte[] inBuf, byte[] outBuf)
		{
			UInt64 a = BytesToUInt64(inBuf, 0);
			UInt64 b = BytesToUInt64(inBuf, 8);

			a = a + S[0];
			b = b + S[1];

			for (int i = 1; i < R + 1; i++)
			{
				a = ROL((a ^ b), (int)b) + S[2 * i];
				b = ROL((b ^ a), (int)a) + S[2 * i + 1];
			}

			UInt64ToBytes(a, outBuf, 0);
			UInt64ToBytes(b, outBuf, 8);
		}
		
		public void Decipher(byte[] inBuf, byte[] outBuf)
		{
			UInt64 a = BytesToUInt64(inBuf, 0);
			UInt64 b = BytesToUInt64(inBuf, 8);

			for (int i = R; i > 0; i--)
			{
				b = ROR((b - S[2 * i + 1]), (int)a) ^ a;
				a = ROR((a - S[2 * i]), (int)b) ^ b;
			}

			b = b - S[1];
			a = a - S[0];

			UInt64ToBytes(a, outBuf, 0);
			UInt64ToBytes(b, outBuf, 8);
		}
	}
}
