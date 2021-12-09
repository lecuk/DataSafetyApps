using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexMD5
{
	static class ByteHelper
	{
		public static byte[] ConcatByteArrays(params byte[][] arrays)
		{
			int finalLength = 0;
			for (int i = 0; i < arrays.Length; ++i)
			{
				finalLength += arrays[i].Length;
			}

			byte[] result = new byte[finalLength];

			int curOffset = 0;
			for (int i = 0; i < arrays.Length; ++i)
			{
				Buffer.BlockCopy(arrays[i], 0, result, curOffset, arrays[i].Length);
				curOffset += arrays[i].Length;
			}

			return result;
		}
	}
}
