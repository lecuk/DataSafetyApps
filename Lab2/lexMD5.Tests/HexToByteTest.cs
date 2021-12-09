using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace lexMD5.Tests
{
	[TestClass]
	public class HexToByteTest
	{
		private void Test(string hex, byte[] expectedBytes)
		{
			HexToByte h2b = new HexToByte();

			byte[] actualBytes = h2b.HexStringToByteArray(hex);

			CollectionAssert.AreEqual(expectedBytes, actualBytes);
		}

		private void TestLength(string hex, int expectedBytesLength)
		{
			HexToByte h2b = new HexToByte();

			byte[] actualBytes = h2b.HexStringToByteArray(hex);

			Assert.AreEqual(expectedBytesLength, actualBytes.Length);
		}

		[TestMethod]
		public void TestEmpty()
		{
			Test("", new byte[]
			{
			});
		}

		[TestMethod]
		public void TestZero()
		{
			Test("00", new byte[]
			{
				0x00
			});
		}

		[TestMethod]
		public void TestSimple()
		{
			Test("0001", new byte[]
			{
				0x00,
				0x01
			});
		}

		[TestMethod]
		public void TestSimpleReversed()
		{
			Test("1000", new byte[]
			{
				0x10,
				0x00
			});
		}

		[TestMethod]
		public void TestSomeValue()
		{
			Test("D41D8CD98F00", new byte[]
			{
				0xD4,
				0x1D,
				0x8C,
				0xD9,
				0x8F,
				0x00
			});
		}

		[TestMethod]
		public void TestLength128()
		{
			TestLength("F96B697D7CB7938D525A2F31AAF161D0", 128 / 8);
		}
	}
}
