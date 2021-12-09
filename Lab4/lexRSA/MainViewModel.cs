using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using lexRC5;

namespace lexRSA
{
    class MainViewModel : ViewModelBase
    {
        private const int EncipherBlockSizeRSA = 64;
        private const int DecipherBlockSizeRSA = 128;

        private readonly RSACryptoServiceProvider rsa;
        private readonly RC5_CBC_Pad_64Bit rc5;
        private readonly lexMD5.MD5 md5;

        public MainViewModel()
        {
            rsa = new RSACryptoServiceProvider();
            rc5 = new RC5_CBC_Pad_64Bit(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            md5 = new lexMD5.MD5();

            ResetRSA();
        }

        private string _inputPath;
        public string InputPath
        {
            get => _inputPath;
            set
            {
                _inputPath = value;
                RaisePropertyChanged();
            }
        }

        private string _outputPath;
        public string OutputPath
        {
            get => _outputPath;
            set
            {
                _outputPath = value;
                RaisePropertyChanged();
            }
        }

        private double _timeElapsed;
        public double TimeElapsed
        {
            get => _timeElapsed;
            set
            {
                _timeElapsed = value;
                RaisePropertyChanged();
            }
        }

        private long _progressBytes;
        public long ProgressBytes
        {
            get => _progressBytes;
            set
            {
                _progressBytes = value;
                RaisePropertyChanged();
            }
        }

        private long _totalBytes;
        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                _totalBytes = value;
                RaisePropertyChanged();
            }
        }

        private string _rsaKeyHash;
        public string RSAKeyHash
        {
            get => _rsaKeyHash;
            set
            {
                _rsaKeyHash = value;
                RaisePropertyChanged();
            }
        }

        private string _rsaKeyFilePath;
        public string RSAKeyFilePath
        {
            get => _rsaKeyFilePath;
            set
            {
                _rsaKeyFilePath = value;
                RaisePropertyChanged();
            }
        }

        private string _rc5Key;
        public string RC5Key
        {
            get => _rc5Key;
            set
            {
                _rc5Key = value;
                RaisePropertyChanged();
            }
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                _isWorking = value;
                RaisePropertyChanged();
            }
        }

        private bool stopRequested;

        public ICommand EncryptRSA => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                IsWorking = true;

                try
                {
                    byte[] input = File.ReadAllBytes(InputPath);
                    byte[] output = EncipherRSAAsync(input);
                    File.WriteAllBytes(OutputPath, output);

                    System.Windows.MessageBox.Show("Successfully encrypted.");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                IsWorking = false;
            });
        });

        public ICommand DecryptRSA => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                IsWorking = true;

                try
                {
                    byte[] input = File.ReadAllBytes(InputPath);
                    byte[] output = DecipherRSAAsync(input);
                    File.WriteAllBytes(OutputPath, output);

                    System.Windows.MessageBox.Show("Successfully decrypted.");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                IsWorking = false;
            });
        });

        public ICommand EncryptRC5 => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                IsWorking = true;

                Stream inputStream = null;
                Stream outputStream = null;

                try
                {
                    inputStream = new FileStream(InputPath, FileMode.Open);
                    outputStream = new FileStream(OutputPath, FileMode.Create);

                    EncipherRC5Async(inputStream, outputStream);

                    System.Windows.MessageBox.Show("Successfully encrypted.");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
                finally
                {
                    inputStream?.Dispose();
                    outputStream?.Dispose();
                }

                IsWorking = false;
            });
        });

        public ICommand DecryptRC5 => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                IsWorking = true;

                Stream inputStream = null;
                Stream outputStream = null;

                try
                {
                    inputStream = new FileStream(InputPath, FileMode.Open);
                    outputStream = new FileStream(OutputPath, FileMode.Create);

                    DecipherRC5Async(inputStream, outputStream);

                    System.Windows.MessageBox.Show("Successfully decrypted.");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
                finally
                {
                    inputStream?.Dispose();
                    outputStream?.Dispose();
                }

                IsWorking = false;
            });
        });

        public ICommand BrowseInput => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    InitialDirectory = Directory.GetCurrentDirectory()
                };
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    InputPath = dialog.FileName;
                }
            });
        });

        public ICommand BrowseOutput => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    InitialDirectory = Directory.GetCurrentDirectory()
                };
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    OutputPath = dialog.FileName;
                }
            });
        });

        public ICommand ExportRSAKey => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    SaveFileDialog dialog = new SaveFileDialog()
                    {
                        InitialDirectory = Directory.GetCurrentDirectory(),
                        FileName = "RSA_Key.xml"
                    };
                    bool? result = dialog.ShowDialog();

                    if (result == true)
                    {
                        string xml = rsa.ToXmlString(includePrivateParameters: true);
                        File.WriteAllText(dialog.FileName, xml);
                        RSAKeyFilePath = dialog.FileName;

                        System.Windows.MessageBox.Show("Successfully exported.");
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand ImportRSAKey => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    OpenFileDialog dialog = new OpenFileDialog()
                    {
                        InitialDirectory = Directory.GetCurrentDirectory()
                    };
                    bool? result = dialog.ShowDialog();

                    if (result == true)
                    {
                        RSAKeyFilePath = dialog.FileName;
                        string xml = File.ReadAllText(dialog.FileName);
                        rsa.FromXmlString(xml);
                        UpdateRSAParametersHash();

                        System.Windows.MessageBox.Show("Successfully imported.");
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand ResetRSAKey => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                ResetRSA();
            });
        });

        public ICommand Stop => new DelegateCommand(() =>
        {
            stopRequested = true;
        });

        private byte[] EncipherRSAAsync(byte[] inputBytes)
        {
            var stopWatch = new Stopwatch();
            var encipheredBytes = new List<byte>
            {
                Capacity = inputBytes.Length * 2
            };

            TotalBytes = inputBytes.LongLength;

            const int updateFrequencyInMillis = 333;
            long lastMillis = 0;
            stopWatch.Start();
            
            for (int i = 0; i < inputBytes.Length; i += EncipherBlockSizeRSA)
            {
                if (stopRequested)
                {
                    stopRequested = false;
                    throw new Exception("Cancelled.");
                }

                var inputBlock = inputBytes
                    .Skip(i)
                    .Take(EncipherBlockSizeRSA)
                    .ToArray();

                encipheredBytes.AddRange(rsa.Encrypt(
                    inputBlock,
                    fOAEP: false));

                if (stopWatch.ElapsedMilliseconds - lastMillis > updateFrequencyInMillis)
                {
                    ProgressBytes = i;
                    TimeElapsed = Math.Round((double)stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond / 1000.0, 2);
                    lastMillis = stopWatch.ElapsedMilliseconds;
                }
            }

            ProgressBytes = inputBytes.LongLength;
            TimeElapsed = Math.Round((double)stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond / 1000.0, 2);

            stopWatch.Stop();

            return encipheredBytes.ToArray();
        }

        private byte[] DecipherRSAAsync(byte[] inputBytes)
        {
            var stopWatch = new Stopwatch();
            var decipheredBytes = new List<byte>
            {
                Capacity = inputBytes.Length / 2
            };
            
            TotalBytes = inputBytes.LongLength;

            const int updateFrequencyInMillis = 333;
            long lastMillis = 0;
            stopWatch.Start();

            for (int i = 0; i < inputBytes.Length; i += DecipherBlockSizeRSA)
            {
                if (stopRequested)
                {
                    stopRequested = false;
                    throw new Exception("Cancelled.");
                }

                var inputBlock = inputBytes
                    .Skip(i)
                    .Take(DecipherBlockSizeRSA)
                    .ToArray();

                decipheredBytes.AddRange(rsa.Decrypt(
                    inputBlock,
                    fOAEP: false));

                if (stopWatch.ElapsedMilliseconds - lastMillis > updateFrequencyInMillis)
                {
                    ProgressBytes = i;
                    TimeElapsed = Math.Round((double)stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond / 1000.0, 2);
                    lastMillis = stopWatch.ElapsedMilliseconds;
                }
            }

            ProgressBytes = inputBytes.LongLength;
            TimeElapsed = Math.Round((double)stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond / 1000.0, 2);
            stopWatch.Stop();

            return decipheredBytes.ToArray();
        }

        private void EncipherRC5Async(Stream inputStream, Stream outputStream)
        {
            UpdateRC5Key();

            var stopWatch = new Stopwatch();

            TotalBytes = inputStream.Length;
            stopWatch.Start();

            rc5.Encrypt(inputStream, outputStream);

            ProgressBytes = inputStream.Length;
            TimeElapsed = Math.Round((double)stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond / 1000.0, 2);
            stopWatch.Stop();
        }
        
        private void DecipherRC5Async(Stream inputStream, Stream outputStream)
        {
            UpdateRC5Key();

            var stopWatch = new Stopwatch();

            TotalBytes = inputStream.Length;
            stopWatch.Start();

            rc5.Decrypt(inputStream, outputStream);

            ProgressBytes = inputStream.Length;
            TimeElapsed = Math.Round((double)stopWatch.ElapsedTicks / TimeSpan.TicksPerMillisecond / 1000.0, 2);
            stopWatch.Stop();
        }

        private void UpdateRSAParametersHash()
        {
            RSAParameters parameters = rsa.ExportParameters(includePrivateParameters: true);

            List<byte> bytes = new List<byte>();

            bytes.AddRange(parameters.D);
            bytes.AddRange(parameters.DP);
            bytes.AddRange(parameters.DQ);
            bytes.AddRange(parameters.Exponent);
            bytes.AddRange(parameters.InverseQ);
            bytes.AddRange(parameters.Modulus);
            bytes.AddRange(parameters.P);
            bytes.AddRange(parameters.Q);

            byte[] hash = md5.MakeHash(bytes.ToArray());

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hash.Length; ++i)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            RSAKeyHash = builder.ToString();
        }

        private void UpdateRC5Key()
        {
            byte[] keyBytes = Encoding.Unicode.GetBytes(RC5Key);
            byte[] hash = md5.MakeHash(keyBytes);

            rc5.Key = hash;
        }

        private void ResetRSA()
        {
            RSA r = RSA.Create();
            RSAParameters parameters = r.ExportParameters(includePrivateParameters: true);
            rsa.ImportParameters(parameters);

            RSAKeyFilePath = "<not saved>";
            UpdateRSAParametersHash();
        }
    }
}
