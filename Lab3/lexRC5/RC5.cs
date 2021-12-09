using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace lexRC5
{
	public class RC5_CBC_Pad_64Bit : IEncryptor<byte[]>, IDisposable
	{
		private short _rounds;
		public short Rounds
		{
			get => _rounds;
			set
			{
				if (value < 0 || value > 255)
				{
					throw new ArgumentException(nameof(value));
				}

				_rounds = value;
				RebuildKey();
			}
		}

		private byte[] _key;
		public byte[] Key
		{
			private get => _key;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (value.Length > 255)
				{
					throw new ArgumentException($"Key length should be [0..255] bytes long");
				}

				_key = value;
				RebuildKey();
			}
		}
		
		public RC5_CBC_Pad_64Bit(byte[] key)
		{
			Key = key;
			Rounds = 8;
		}

		public RC5_CBC_Pad_64Bit() : this(new byte[0])
		{
		}

		public void Encrypt(Stream source, Stream destination)
		{
			Random r = new Random();
			Encrypt(source, destination, (ulong)r.Next() << 32 + r.Next(), (ulong)r.Next() << 32 + r.Next());
		}

		public void Encrypt(Stream source, Stream destination, UInt64 initVectorA, UInt64 initVectorB)
		{
			UInt64 blockA, blockB, 
				prevBlockA = initVectorA,
				prevBlockB = initVectorB,
				firstBlockA = initVectorA,
				firstBlockB = initVectorB;
			byte[] bytesA, bytesB;

			// write initialization vector as first block in ECB mode.
			CipherBlock(ref firstBlockA, ref firstBlockB);

			bytesA = BitConverter.GetBytes(firstBlockA);
			bytesB = BitConverter.GetBytes(firstBlockB);

			destination.Write(bytesA, 0, BytesInWord);
			destination.Write(bytesB, 0, BytesInWord);

			bool hasNextBlock = true;
			do
			{
				hasNextBlock = GetNextHalfBlocksFromRawStream(source, out blockA, out blockB);

				blockA ^= prevBlockA;
				blockB ^= prevBlockB;
				
				CipherBlock(ref blockA, ref blockB);

				bytesA = BitConverter.GetBytes(blockA);
				bytesB = BitConverter.GetBytes(blockB);

				destination.Write(bytesA, 0, BytesInWord);
				destination.Write(bytesB, 0, BytesInWord);

				prevBlockA = blockA;
				prevBlockB = blockB;
			}
			while (hasNextBlock);
		}

		public void Decrypt(Stream source, Stream destination)
		{
			UInt64 blockA, blockB, prevBlockA, prevBlockB;
			
			if (!GetNextHalfBlocksFromEncryptedStream(source, out prevBlockA, out prevBlockB))
			{
				throw new InvalidOperationException("File is corrupt.");
			}

			DecipherBlock(ref prevBlockA, ref prevBlockB);

			if (!GetNextHalfBlocksFromEncryptedStream(source, out blockA, out blockB))
			{
				throw new InvalidOperationException("File is corrupt.");
			}

			bool hasNextBlock = true;
			while (hasNextBlock)
			{
				UInt64 
					tempBlockA = blockA,
					tempBlockB = blockB;

				DecipherBlock(ref blockA, ref blockB);

				blockA ^= prevBlockA;
				blockB ^= prevBlockB;

				byte[] bytesA = BitConverter.GetBytes(blockA);
				byte[] bytesB = BitConverter.GetBytes(blockB);

				prevBlockA = tempBlockA;
				prevBlockB = tempBlockB;

				hasNextBlock = GetNextHalfBlocksFromEncryptedStream(source, out blockA, out blockB);

				if (hasNextBlock)
				{
					destination.Write(bytesA, 0, BytesInWord);
					destination.Write(bytesB, 0, BytesInWord);
				}
				else
				{
					int bytesAdded = bytesB[BytesInWord - 1];

					if (bytesAdded < BytesInWord)
					{
						destination.Write(bytesA, 0, BytesInWord);
						destination.Write(bytesB, 0, BytesInWord - bytesAdded);
					}
					else
					{
						destination.Write(bytesA, 0, BytesInWord - (bytesAdded - BytesInWord));
					}
				}
			}
		}

		public void Dispose()
		{
			// destroy sensitive information
			for (int i = 0; i < Key.Length; ++i)
			{
				Key[i] = 0;
			}

			for (int i = 0; i < keyWords.Length; ++i)
			{
				keyWords[i] = 0;
			}

			for (int i = 0; i < finalKey.Length; ++i)
			{
				finalKey[i] = 0;
			}

			Key = new byte[0];
			keyWords = new UInt64[0];
			finalKey = new UInt64[0];
		}

		private const UInt64 
			P = 0xb7e151628aed2a6b, 
			Q = 0x9e3779b97f4a7c15;

		private const int BytesInWord = 64 / 8;
		private const int BytesInBlock = BytesInWord * 2;

		private UInt64[] keyWords, finalKey;
		private int wordCount, finalKeyLength;

		private void RebuildKey()
		{
			unchecked
			{
				ConvertKeyToWords();
				InitFinalKey();
				MixFinalKeyWithWords();
			}
		}

		private void ConvertKeyToWords()
		{
			wordCount = (int)Math.Ceiling((decimal)Key.Length / BytesInWord);
			keyWords = new UInt64[wordCount];

			for (int i = 0; i < wordCount; ++i)
			{
				keyWords[i] = 0;
			}

			for (int i = Key.Length - 1; i >= 0; --i)
			{
				keyWords[i / BytesInWord] = (keyWords[i / BytesInWord] << 8) + Key[i];
			}
		}

		private void InitFinalKey()
		{
			finalKeyLength = (Rounds + 1) * 2;
			finalKey = new UInt64[finalKeyLength];

			finalKey[0] = P;
			for (int i = 1; i < finalKeyLength; ++i)
			{
				finalKey[i] = finalKey[i - 1] + Q;
			}
		}
		
		private void MixFinalKeyWithWords()
		{
			int i = 0, j = 0;
			UInt64 A = 0, B = 0;
			int t = Math.Max(wordCount, finalKeyLength);

			for (int s = 0; s < 3 * t; ++s)
			{
				A = finalKey[i] = RotateLeft(finalKey[i] + A + B, 3);
				B = keyWords[j] = RotateLeft(keyWords[j] + A + B, (A + B));
				i = (i + 1) % finalKeyLength;
				j = (j + 1) % wordCount;
			}
		}

		private UInt64 RotateLeft(UInt64 x, int shift)
		{
			UInt64 r1 = x << shift;
			UInt64 r2 = x >> (64 - shift);
			return (r1 | r2);
		}

		private UInt64 RotateLeft(UInt64 x, UInt64 shift)
		{
			return RotateLeft(x, (int)shift);
		}

		private UInt64 RotateRight(UInt64 x, int shift)
		{
			UInt64 r1 = x >> shift;
			UInt64 r2 = x << (64 - shift);
			return (r1 | r2);
		}

		private UInt64 RotateRight(UInt64 x, UInt64 shift)
		{
			return RotateRight(x, (int)shift);
		}

		private void CipherBlock(ref UInt64 blockA, ref UInt64 blockB)
		{
			unchecked
			{
				blockA += finalKey[0];
				blockB += finalKey[1];

				for (int r = 1; r < Rounds + 1; ++r)
				{
					blockA = RotateLeft(blockA ^ blockB, blockB);
					blockA += finalKey[2 * r];

					blockB = RotateLeft(blockB ^ blockA, blockA);
					blockB += finalKey[2 * r + 1];
				}
			}
		}
		
		private void DecipherBlock(ref UInt64 blockA, ref UInt64 blockB)
		{
			unchecked
			{
				for (int r = Rounds; r > 0; --r)
				{
					blockB -= finalKey[2 * r + 1];
					blockB = RotateRight(blockB, blockA) ^ blockA;

					blockA -= finalKey[2 * r];
					blockA = RotateRight(blockA, blockB) ^ blockB;
				}

				blockA -= finalKey[0];
				blockB -= finalKey[1];
			}
		}

		private bool GetNextHalfBlocksFromRawStream(Stream input, out UInt64 blockA, out UInt64 blockB)
		{
			byte[] buffer = new byte[BytesInBlock];
			for (int i = 0; i < BytesInBlock; ++i)
			{
				buffer[i] = 0;
			}

			int bytesRead = input.Read(buffer, 0, BytesInBlock);

			if (bytesRead < BytesInBlock)
			{
				FillBufferWithTailBytes(buffer, bytesRead);
			}

			GetHalfBlocksFromBytes(buffer, out blockA, out blockB);

			return (bytesRead == BytesInBlock);
		}

		private bool GetNextHalfBlocksFromEncryptedStream(Stream input, out UInt64 blockA, out UInt64 blockB)
		{
			byte[] buffer = new byte[BytesInBlock];
			for (int i = 0; i < BytesInBlock; ++i)
			{
				buffer[i] = 0;
			}

			int bytesRead = input.Read(buffer, 0, BytesInBlock);

			GetHalfBlocksFromBytes(buffer, out blockA, out blockB);

			return (bytesRead == BytesInBlock);
		}

		private void FillBufferWithTailBytes(byte[] buffer, int bytesRead)
		{
			for (int i = bytesRead; i < BytesInBlock; ++i)
			{
				buffer[i] = (byte)(BytesInBlock - bytesRead);
			}
		}

		private void GetHalfBlocksFromBytes(byte[] buffer, out UInt64 blockA, out UInt64 blockB)
		{
			blockA = BitConverter.ToUInt64(buffer, 0);
			blockB = BitConverter.ToUInt64(buffer, BytesInWord);
		}
	}
}
