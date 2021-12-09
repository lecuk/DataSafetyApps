using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexMD5
{
	public interface IHashAlgorithm
	{
		int HashLengthInBits { get; }
		byte[] MakeHash(byte[] data);
		byte[] MakeHash(Stream data);
	}
}

