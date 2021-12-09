using System;
using System.Text;

namespace lexMD5
{
	public class HexToByte
	{
		private int GetHexByteValue(char hexChar)
		{
			if (hexChar >= '0' && hexChar <= '9')
			{
				return hexChar - '0';
			}

			if (hexChar >= 'a' && hexChar <= 'f')
			{
				return hexChar - 'a' + 10;
			}

			if (hexChar >= 'A' && hexChar <= 'F')
			{
				return hexChar - 'A' + 10;
			}

			throw new ArgumentException(String.Format("Not a hexadecimal character: {0}", hexChar));
		}

		public byte[] HexStringToByteArray(string hex)
		{
			if (hex.Length % 2 != 0)
			{
				throw new Exception("The binary key cannot have an odd number of digits");
			}

			byte[] data = new byte[hex.Length / 2];
			for (int i = 0; i < data.Length; i++)
			{
				int a = GetHexByteValue(hex[i * 2]) << 4;
				int b = GetHexByteValue(hex[i * 2 + 1]);

				data[i] = (byte)(a | b);
			}

			return data;
		}

		public string ByteToHex(byte[] data)
		{
			StringBuilder hex = new StringBuilder(data.Length * 2);
			foreach (byte b in data)
			{
				hex.AppendFormat("{0:X2}", b);
			}
			return hex.ToString();
		}
	}
}
