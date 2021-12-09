using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexMD5
{
	internal sealed class MD5Buffer
	{
		public uint A;
		public uint B;
		public uint C;
		public uint D;

		public MD5Buffer()
		{
			this.A = 0x67452301;
			this.B = 0xefcdab89;
			this.C = 0x98badcfe;
			this.D = 0x10325476;
		}

		public override string ToString()
		{
			return String.Format("{0}-{1}-{2}-{3}",
				A.ToString("X2").PadLeft(8, '0'),
				B.ToString("X2").PadLeft(8, '0'),
				C.ToString("X2").PadLeft(8, '0'),
				D.ToString("X2").PadLeft(8, '0'));
		}

		public byte[] ToBytes()
		{
			byte[] aBytes = BitConverter.GetBytes(A);
			byte[] bBytes = BitConverter.GetBytes(B);
			byte[] cBytes = BitConverter.GetBytes(C);
			byte[] dBytes = BitConverter.GetBytes(D);

			return ByteHelper.ConcatByteArrays(aBytes, bBytes, cBytes, dBytes);
		}
	}
}
