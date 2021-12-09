using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexMD5
{
	internal sealed class MD5Round
	{
		private int[] S { get; set; }
		private uint[] T { get; set; }
		private Func<int, int> IndexCalculator { get; set; }
		private Func<uint, uint, uint, uint> BitFunction { get; set; }

		public void ExecuteRound(uint[] block, ref uint a, ref uint b, ref uint c, ref uint d)
		{
			for (int i = 0; i < 16; ++i)
			{
				int j = IndexCalculator(i);
				a = RoundPart(a, b, c, d, block[j], S[i % 4], T[i]);
				Swap(ref a, ref b, ref c, ref d);
			}
		}
		
		private uint RoundPart(uint a, uint b, uint c, uint d, uint x, int s, uint t)
		{
			unchecked
			{
				uint bits = a + (BitFunction(b, c, d)) + x + t;
				return b + RotateLeft(bits, s);
			}
		}

		private uint RotateLeft(uint i, int s)
		{
			return (i << s) | (i >> (32 - s));
		}

		private void Swap(ref uint a, ref uint b, ref uint c, ref uint d)
		{
			uint temp = d;
			d = c;
			c = b;
			b = a;
			a = temp;
		}

		public static MD5Round Round1 = new MD5Round()
		{
			S = new int[4]
			{
				7, 12, 17, 22
			},
			T = new uint[16]
			{
				0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
				0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
				0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
				0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821
			},
			IndexCalculator = (i) =>
			{
				return i;
			},
			BitFunction = (b, c, d) =>
			{
				return (b & c) | (~b & d);
			}
		};

		public static MD5Round Round2 = new MD5Round()
		{
			S = new int[4]
			{
				5, 9, 14, 20
			},
			T = new uint[16]
			{
				0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
				0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
				0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
				0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a
			},
			IndexCalculator = (i) =>
			{
				return (1 + 5 * i) % 16;
			},
			BitFunction = (b, c, d) =>
			{
				return (b & d) | (c & ~d);
			}
		};

		public static MD5Round Round3 = new MD5Round()
		{
			S = new int[4]
			{
				4, 11, 16, 23
			},
			T = new uint[16]
			{
				0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
				0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
				0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
				0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665
			},
			IndexCalculator = (i) =>
			{
				return (5 + 3 * i) % 16;
			},
			BitFunction = (b, c, d) =>
			{
				return (b ^ c ^ d);
			}
		};

		public static MD5Round Round4 = new MD5Round()
		{
			S = new int[4]
			{
				6, 10, 15, 21
			},
			T = new uint[16]
			{
				0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
				0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
				0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
				0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
			},
			IndexCalculator = (i) =>
			{
				return (7 * i) % 16;
			},
			BitFunction = (b, c, d) =>
			{
				return (c ^ (b | ~d));
			}
		};
	}
}
