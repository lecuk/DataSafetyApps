using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexMD5.ConsoleTestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			IHashAlgorithm algorithm = new MD5();
			HexToByte hexToByte = new HexToByte();

			while (true)
			{
				string input = Console.ReadLine();
				byte[] bytes = Encoding.UTF8.GetBytes(input);
				byte[] hash = algorithm.MakeHash(bytes);
				string hex = hexToByte.ByteToHex(hash);
				Console.WriteLine(hex);
			}
		}
	}
}
