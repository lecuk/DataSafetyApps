using System;
using System.IO;

namespace lexMD5
{
	public class MD5 : IHashAlgorithm
	{
		public int HashLengthInBits => 128;

		private const int BitsPerByte = 8;
		private const int BlockSize = 512;
		private const int BlockSizeInBytes = BlockSize / BitsPerByte;
		private const int DataLengthPadding = 64;
		private const int DataLengthPaddingInBytes = DataLengthPadding / BitsPerByte;
		private const int WordSize = 32;
		private const int WordSizeInBytes = WordSize / BitsPerByte;
		private const int WordsPerBlock = BlockSize / WordSize;

		public byte[] MakeHash(byte[] data)
		{
			return MakeHash(new MemoryStream(data));
		}

		public byte[] MakeHash(Stream stream)
		{
			if (!stream.CanRead)
			{
				throw new InvalidOperationException("The stream should be accesible for reading.");
			}

			MD5Buffer buffer = new MD5Buffer();
			long streamLength = stream.Length;
			byte[] tail = CreateTail(streamLength);
			Stream tailStream = new MemoryStream(tail);

			byte[] block = new byte[BlockSizeInBytes];
			uint[] wordBlock = new uint[WordsPerBlock];
			bool hasNext = true;

			do
			{
				hasNext = ReadNextBlock(stream, tailStream, ref block, streamLength);
				FillWord(block, ref wordBlock);
				DigestBlock(wordBlock, buffer);
			}
			while (hasNext);

			return buffer.ToBytes();
		}

		// returns true if block is returned
		private bool ReadNextBlock(Stream stream, Stream tailStream, ref byte[] block, long streamLength)
		{
			int bytesRead = 0;
			if (stream.Position < streamLength)
			{
				bytesRead = stream.Read(block, 0, BlockSizeInBytes);
			}

			if (bytesRead < BlockSizeInBytes)
			{
				bytesRead += tailStream.Read(block, bytesRead, BlockSizeInBytes - bytesRead);
			}

			bool end = (tailStream.Position == tailStream.Length);

			if (end)
			{
				return false;
			}

			if (bytesRead == BlockSizeInBytes)
			{
				return true;
			}

			throw new Exception("tail size is shitty you idiot");
		}

		private byte[] CreateTail(long streamLength)
		{
			long lengthInBits = streamLength * BitsPerByte;
			long nextLength = (lengthInBits + DataLengthPadding) / BlockSize * BlockSize + BlockSize; //division and multiplication is important
			long bitCountToAdd = nextLength - lengthInBits;
			long byteCountToAdd = Math.Max(1, bitCountToAdd / BitsPerByte);

			byte[] tail = new byte[byteCountToAdd];
			tail[0] = 0b10000000;
			for (int i = 1; i < tail.Length; ++i)
			{
				tail[i] = 0x00;
			}

			byte[] lengthBytes = BitConverter.GetBytes(lengthInBits);
			for (int i = 0; i < 8; ++i)
			{
				// insert in reversed order
				tail[tail.Length - 8 + i] = lengthBytes[i];
			}

			return tail;
		}

		private void FillWord(byte[] block, ref uint[] wordBlock)
		{
			if (block.Length != BlockSizeInBytes)
			{
				throw new ArgumentException("Block size != 128");
			}

			for (var i = 0; i < WordsPerBlock; i++)
			{
				uint word = 0;
				word += (uint)block[i * 4 + 0];
				word += (uint)block[i * 4 + 1] << 8;
				word += (uint)block[i * 4 + 2] << 16;
				word += (uint)block[i * 4 + 3] << 24;

				wordBlock[i] = word;
			}
		}

		private void DigestBlock(uint[] block, MD5Buffer buffer)
		{
			uint a = buffer.A;
			uint b = buffer.B;
			uint c = buffer.C;
			uint d = buffer.D;

			var r = new MD5Buffer();

			MD5Round.Round1.ExecuteRound(block, ref a, ref b, ref c, ref d);
			MD5Round.Round2.ExecuteRound(block, ref a, ref b, ref c, ref d);
			MD5Round.Round3.ExecuteRound(block, ref a, ref b, ref c, ref d);
			MD5Round.Round4.ExecuteRound(block, ref a, ref b, ref c, ref d);

			unchecked
			{
				buffer.A += a;
				buffer.B += b;
				buffer.C += c;
				buffer.D += d;
			}
		}
	}
}
