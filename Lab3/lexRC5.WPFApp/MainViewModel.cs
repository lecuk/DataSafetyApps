using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using lexLinearRandomGenerator;
using lexMD5;
using lexRC5;

namespace lexRC5
{
	class MainViewModel : ViewModelBase
	{
		private string _inputPath;
		public string InputPath
		{
			get => _inputPath ?? String.Empty;
			set
			{
				_inputPath = value;
				RaisePropertyChanged(nameof(InputPath));
			}
		}

		private string _outputPath;
		public string OutputPath
		{
			get => _outputPath ?? String.Empty;
			set
			{
				_outputPath = value;
				RaisePropertyChanged(nameof(OutputPath));
			}
		}

		private string _key;
		public string Key
		{
			get => _key ?? String.Empty;
			set
			{
				_key = value;
				RaisePropertyChanged(nameof(Key));
			}
		}

		private int _rounds;
		public int Rounds
		{
			get => _rounds;
			set
			{
				_rounds = value;
				RaisePropertyChanged(nameof(Rounds));
			}
		}

		public ICommand Cipher => new DelegateCommand(() =>
		{
			try
			{
				LinearIntegerGenerator random = new LinearIntegerGenerator(19, 1 << 28 - 1, 15 * 15 * 15, 4181);

				byte[] keyBytes = GenerateKeyBytes();

				RC5_CBC_Pad_64Bit rc5 = new RC5_CBC_Pad_64Bit(keyBytes)
				{
					Rounds = (short)Rounds
				};

				using (Stream input = new FileStream(InputPath, FileMode.Open))
				{
					using (Stream output = new FileStream(OutputPath, FileMode.Create))
					{
						rc5.Encrypt(input, output, (ulong)random.Next(), (ulong)random.Next());
					}
				}

				ErrorMessage = String.Empty;
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
			}
		});

		public ICommand Decipher => new DelegateCommand(() =>
		{
			try
			{
				byte[] keyBytes = GenerateKeyBytes();

				RC5_CBC_Pad_64Bit rc5 = new RC5_CBC_Pad_64Bit(keyBytes)
				{
					Rounds = (short)Rounds
				};

				using (Stream input = new FileStream(InputPath, FileMode.Open))
				{
					using (Stream output = new FileStream(OutputPath, FileMode.Create))
					{
						rc5.Decrypt(input, output);
					}
				}

				ErrorMessage = String.Empty;
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
			}
		});

		private byte[] GenerateKeyBytes()
		{
			MD5 md5 = new MD5();

			byte[] keyBytes1 = md5.MakeHash(Encoding.Unicode.GetBytes(Key));
			byte[] keyBytes2 = md5.MakeHash(keyBytes1);

			byte[] result = new byte[32];

			for (int i = 0; i < 16; ++i)
			{
				result[i] = keyBytes1[i];
				result[i + 16] = keyBytes2[i];
			}

			return result;
		}

		public MainViewModel()
		{
			Rounds = 8;
		}
	}
}
