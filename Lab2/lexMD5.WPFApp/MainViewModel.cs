using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace lexMD5.WPFApp
{
	class MainViewModel : ViewModelBase
	{
		public MainViewModel()
		{
			DataMode = DataMode.FromText;
			TextEncoding = Encoding.UTF8;

			stopwatch = new Stopwatch();
		}

		private readonly Stopwatch stopwatch;

		private DataMode _dataMode;
		public DataMode DataMode
		{
			get => _dataMode;
			set
			{
				_dataMode = value;
				RaisePropertyChanged(nameof(DataMode));
			}
		}

		private string _textToBeHashed;
		public string TextToBeHashed
		{
			get => _textToBeHashed;
			set
			{
				_textToBeHashed = value;
				RaisePropertyChanged(nameof(TextToBeHashed));
			}
		}

		private string _hash;
		public string Hash
		{
			get => _hash;
			set
			{
				_hash = value;
				RaisePropertyChanged(nameof(Hash));
			}
		}

		private string _filePath;
		public string FilePath
		{
			get => _filePath;
			set
			{
				_filePath = value;
				RaisePropertyChanged(nameof(FilePath));
			}
		}

		private string _filePreview;
		public string FilePreview
		{
			get => _filePreview;
			set
			{
				_filePreview = value;
				RaisePropertyChanged(nameof(FilePreview));
			}
		}

		private Encoding _textEncoding;
		public Encoding TextEncoding
		{
			get => _textEncoding ?? Encoding.UTF8;
			set
			{
				_textEncoding = value;
				RaisePropertyChanged(nameof(TextEncoding));
			}
		}

		private bool _isRunning;
		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				_isRunning = value;
				RaisePropertyChanged(nameof(IsRunning));
			}
		}

		private double _millisSpentOnHash;
		public double MillisSpentOnHash
		{
			get => _millisSpentOnHash;
			set
			{
				_millisSpentOnHash = value;
				RaisePropertyChanged(nameof(MillisSpentOnHash));
			}
		}

		public ICommand LoadPreview => new DelegateCommand(() =>
		{
			int previewLines = 20;
			try
			{
				using (Stream file = File.Open(FilePath, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(file, TextEncoding))
					{
						StringBuilder builder = new StringBuilder();

						for (int i = 0; i < previewLines; ++i)
						{
							if (reader.EndOfStream)
							{
								break;
							}

							builder.Append(reader.ReadLine());
						}

						FilePreview = builder.ToString();
					}
				}

				ResetError();
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
				Hash = String.Empty;
			}
		});

		public ICommand GenerateHash => new DelegateCommand(() =>
		{
			if (DataMode == DataMode.FromText)
			{
				GenerateHashFromText();
			}
			else if (DataMode == DataMode.FromFile)
			{
				GenerateHashFromFile();
			}
		});

		public ICommand Browse => new DelegateCommand(() =>
		{
			OpenFileDialog dialog = new OpenFileDialog()
			{
				Filter = "All files|*.*|Text files|*.txt",
				CheckPathExists = true,
				CheckFileExists = true
			};

			if (dialog.ShowDialog() == true)
			{
				FilePath = dialog.FileName;
			}
		});

		private void GenerateHashFromText()
		{
			IHashAlgorithm hasher = new MD5();
			HexToByte hex2byte = new HexToByte();

			Task.Run(() =>
			{
				string text = TextToBeHashed ?? String.Empty;
				byte[] data = TextEncoding.GetBytes(text);

				HashStarted();
				byte[] hash = hasher.MakeHash(data);
				string hex = hex2byte.ByteToHex(hash);
				HashSuccessfullyFinished(hex);
			});
		}

		private void GenerateHashFromFile()
		{
			IHashAlgorithm hasher = new MD5();
			HexToByte hex2byte = new HexToByte();

			Task.Run(() =>
			{
				try
				{
					string path = FilePath ?? String.Empty;
					using (Stream file = File.Open(path, FileMode.Open))
					{
						HashStarted();
						byte[] hash = hasher.MakeHash(file);
						string hex = hex2byte.ByteToHex(hash);
						HashSuccessfullyFinished(hex);
					}
				}
				catch (Exception ex)
				{
					HashFinishedWithError(ex.Message);
				}
			});
		}

		private void HashStarted()
		{
			IsRunning = true;
			Hash = "...";
			stopwatch.Restart();
		}

		private void HashSuccessfullyFinished(string hash)
		{
			stopwatch.Stop();
			MillisSpentOnHash = (double)stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;
			ResetError();
			IsRunning = false;
			Hash = hash;
		}

		private void HashFinishedWithError(string error)
		{
			stopwatch.Stop();
			MillisSpentOnHash = 0;
			ErrorMessage = error;
			Hash = String.Empty;
		}
	}
}
