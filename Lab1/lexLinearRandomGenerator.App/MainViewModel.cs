using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace lexLinearRandomGenerator.App
{
	internal class MainViewModel : ViewModelBase
	{
		private uint _state;
		public uint State
		{
			get => _state;
			set
			{
				_state = value;
				RaisePropertyChanged(nameof(State));
			}
		}

		private uint _m;
		public uint M
		{
			get => _m;
			set
			{
				_m = value;
				RaisePropertyChanged(nameof(M));
			}
		}

		private uint _a;
		public uint A
		{
			get => _a;
			set
			{
				_a = value;
				RaisePropertyChanged(nameof(A));
			}
		}

		private uint _c;
		public uint C
		{
			get => _c;
			set
			{
				_c = value;
				RaisePropertyChanged(nameof(C));
			}
		}

		private string _outputFilePath;
		public string OutputFilePath
		{
			get => _outputFilePath;
			set
			{
				_outputFilePath = value;
				RaisePropertyChanged(nameof(OutputFilePath));
			}
		}

		private IEnumerable<uint> _outputSequence;
		public IEnumerable<uint> OutputSequence
		{
			get => _outputSequence;
			set
			{
				_outputSequence = value;
				RaisePropertyChanged(nameof(OutputSequence));
				GenerateOutputSequenceString();
			}
		}

		private string _outputSequenceString;
		public string OutputSequenceString
		{
			get => _outputSequenceString;
			set
			{
				_outputSequenceString = value;
				RaisePropertyChanged(nameof(OutputSequenceString));
			}
		}

		private uint _displayLimit;
		public uint DisplayLimit
		{
			get => _displayLimit;
			set
			{
				_displayLimit = value;
				RaisePropertyChanged(nameof(DisplayLimit));
			}
		}

		private uint _generationLimit;
		public uint GenerationLimit
		{
			get => _generationLimit;
			set
			{
				_generationLimit = value;
				RaisePropertyChanged(nameof(GenerationLimit));
			}
		}

		private uint _period;
		public uint Period
		{
			get => _period;
			set
			{
				_period = value;
				RaisePropertyChanged(nameof(Period));
			}
		}

		private long _elapsed;
		public long ElapsedMilliseconds
		{
			get => _elapsed;
			set
			{
				_elapsed = value;
				RaisePropertyChanged(nameof(ElapsedMilliseconds));
			}
		}

		private string _error;
		public string Error
		{
			get => _error;
			set
			{
				_error = value;
				RaisePropertyChanged(nameof(Error));
			}
		}

		public ICommand CreateSequenceCommand => new DelegateCommand(() =>
		{
			GenerateSequence();
		});

		public ICommand SelectPathCommand => new DelegateCommand(() =>
		{
			SaveFileDialog dialog = new SaveFileDialog()
			{
				DefaultExt = "txt",
				OverwritePrompt = false,
				Filter = "Text files|*.txt|All files|*.*"
			};

			bool result = dialog.ShowDialog() ?? false;
			if (result)
			{
				OutputFilePath = dialog.FileName;
			}
		});

		private IRandomGenerator<uint> CreateGenerator()
		{
			return new LinearIntegerGenerator(State, M, A, C);
		}

		private void GenerateSequence()
		{
			IRandomGenerator<uint> generator = null;

			try
			{
				generator = CreateGenerator();
				ResetError();
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Error = ex.Message;
				return;
			}

			if (!String.IsNullOrWhiteSpace(OutputFilePath))
			{
				GenerateSequenceToWindowAndFile(generator);
			}
			else
			{
				GenerateSequenceToWindow(generator);
			}
		}

		private void GenerateSequenceToWindow(IRandomGenerator<uint> generator)
		{
			IList<uint> outputSequence = null;

			try
			{
				outputSequence = new List<uint>((int)DisplayLimit);
				ResetError();
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Error = ex.Message;
				return;
			}

			uint[] firstNumbers = new uint[3];
			uint first = State;
			firstNumbers[0] = first;
			uint previous = first;
			outputSequence.Add(first);

			uint period = 1;

			Stopwatch watch = new Stopwatch();

			watch.Start();
			for (; period < GenerationLimit; ++period)
			{
				uint next = generator.Next();
				
				if (period < DisplayLimit)
				{
					outputSequence.Add(next);
				}

				if (next == first ||
					previous == next ||
					firstNumbers[0] == next ||
					firstNumbers[1] == next ||
					firstNumbers[2] == next) break;

				if (period < 3)
				{
					firstNumbers[period] = next;
				}
				previous = next;
			}
			watch.Stop();

			ElapsedMilliseconds = watch.ElapsedMilliseconds;
			Period = period;
			OutputSequence = outputSequence;
		}

		private void GenerateSequenceToWindowAndFile(IRandomGenerator<uint> generator)
		{
			IList<uint> outputSequence = null;

			try
			{
				outputSequence = new List<uint>((int)DisplayLimit);
				ResetError();
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Error = ex.Message;
				return;
			}

			try
			{
				Stopwatch watch = new Stopwatch();
				watch.Start();

				using (Stream stream = new FileStream(OutputFilePath, FileMode.Create))
				{
					using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
					{
						uint[] firstNumbers = new uint[3];
						uint first = State;
						firstNumbers[0] = first;
						uint previous = first;
						
						const uint builderCapacity = 10000;
						const int averageNumberLength = 12;
						uint capacity = Math.Min(builderCapacity, GenerationLimit);
						StringBuilder builder = new StringBuilder((int)capacity * averageNumberLength);
						int builderNumberCount = 1;

						outputSequence.Add(first);
						builder.AppendFormat("{0} ", first);

						uint period = 1;

						for (; period < GenerationLimit; ++period)
						{
							uint next = generator.Next();

							if (period < DisplayLimit)
							{
								outputSequence.Add(next);
							}
							
							builder.AppendFormat("{0} ", next);
							if (builderNumberCount >= capacity)
							{
								writer.Write("{0} ", builder.ToString());
								builder.Clear();
								builderNumberCount = 0;
							}

							builderNumberCount++;

							if (next == first || 
								previous == next || 
								firstNumbers[0] == next ||
								firstNumbers[1] == next ||
								firstNumbers[2] == next) break;

							if (period < 3)
							{
								firstNumbers[period] = next;
							}
							previous = next;
						}

						if (builderNumberCount > 0)
						{
							writer.Write("{0} ", builder.ToString());
						}

						Period = period;
					}
				}
				watch.Stop();

				ResetError();
				ElapsedMilliseconds = watch.ElapsedMilliseconds;
				OutputSequence = outputSequence;

				MessageBoxResult result2 = MessageBox.Show("Results successfully generated to the file. Do you want to see the file?", "Success", MessageBoxButton.YesNo);

				if (result2 == MessageBoxResult.Yes)
				{
					Process.Start(OutputFilePath);
				}
			}
			catch (IOException ex)
			{
				Error = ex.Message;
			}
		}

		private void ResetError()
		{
			Error = String.Empty;
		}
		
		private void GenerateOutputSequenceString()
		{
			StringBuilder builder = new StringBuilder();

			foreach (uint number in OutputSequence)
			{
				builder.AppendFormat("{0} ", number);
			}

			OutputSequenceString = builder.ToString();
		}
	}
}
