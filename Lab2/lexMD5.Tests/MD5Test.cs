using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace lexMD5.Tests
{
	[TestClass]
	public class MD5Test
	{
		private void Test(byte[] inputBytes, byte[] expectedHashBytes)
		{
			IHashAlgorithm algorithm = new MD5();
			HexToByte hex2Byte = new HexToByte();

			byte[] actualHashBytes = algorithm.MakeHash(inputBytes);

			string hexExpected = hex2Byte.ByteToHex(expectedHashBytes);
			string hexActual = hex2Byte.ByteToHex(actualHashBytes);

			CollectionAssert.AreEqual(expectedHashBytes, actualHashBytes);
		}

		private void Test(string input, string expectedHash)
		{
			HexToByte hex2Byte = new HexToByte();

			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
			byte[] expectedHashBytes = hex2Byte.HexStringToByteArray(expectedHash);

			Test(inputBytes, expectedHashBytes);
		}

		private void TestWithOriginalMD5(string input)
		{
			HexToByte hex2Byte = new HexToByte();
			var md5 = System.Security.Cryptography.MD5.Create();

			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
			byte[] expectedHashBytes = md5.ComputeHash(inputBytes);

			Test(inputBytes, expectedHashBytes);
		}

		[TestMethod]
		public void TestEmpty()
		{
			Test("", "D41D8CD98F00B204E9800998ECF8427E");
		}

		[TestMethod]
		public void TestA()
		{
			Test("a", "0CC175B9C0F1B6A831C399E269772661");
		}

		[TestMethod]
		public void TestAlpha()
		{
			Test("abcdefghijklmnopqrstuvwxyz", "C3FCD3D76192E4007DFB496CCA67E13B");
		}

		[TestMethod]
		public void TestAlphaNumeric()
		{
			Test("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "D174AB98D277D9F5A5611C2C9F419D9F");
		}

		[TestMethod]
		public void TestRealMD5()
		{
			TestWithOriginalMD5("a");
		}
	}
}
