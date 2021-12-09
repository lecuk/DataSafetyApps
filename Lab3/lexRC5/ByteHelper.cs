using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexRC5
{
	static class ByteHelper
	{
		public static uint RotateLeft(uint x, int shift)
		{
			uint r1, r2;
			r1 = x << shift;
			r2 = x >> (sizeof(uint) - shift);
			return (r1 | r2);
		}

		public static uint RotateRight(uint x, int shift)
		{
			uint r1, r2;
			r1 = x >> shift;
			r2 = x << (sizeof(uint) - shift);
			return (r1 | r2);
		}
	}
}
