using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace lexDSS
{
    class MainViewModel : ViewModelBase
    {
        private readonly DSACryptoServiceProvider dsa;
        private readonly SHA1CryptoServiceProvider sha;

        private readonly Encoding SignatureEncoding = Encoding.ASCII;
        private readonly string HexFormat = "{0:X2}";

        public MainViewModel()
        {
            dsa = new DSACryptoServiceProvider();
            sha = new SHA1CryptoServiceProvider();
        }

        private string _sourceFilePath;
        public string SourceFilePath
        {
            get => _sourceFilePath;
            set
            {
                _sourceFilePath = value;
                IsVerified = null;
                RaisePropertyChanged();
            }
        }

        private string _sourceText;
        public string SourceText
        {
            get => _sourceText;
            set
            {
                _sourceText = value;
                RaisePropertyChanged();
            }
        }

        private string _signaturePath;
        public string SignaturePath
        {
            get => _signaturePath;
            set
            {
                _signaturePath = value;
                IsVerified = null;
                RaisePropertyChanged();
            }
        }

        private string _signature;
        public string Signature
        {
            get => _signature;
            set
            {
                _signature = value;
                RaisePropertyChanged();
            }
        }

        private bool? _isVerified;
        public bool? IsVerified
        {
            get => _isVerified;
            set
            {
                _isVerified = value;
                RaisePropertyChanged();
            }
        }

        public ICommand SignFromFile => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] data = File.ReadAllBytes(SourceFilePath);
                    byte[] signature = SignData(data);

                    Signature = ByteToHex(signature);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand SignFromText => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] data = Encoding.Default.GetBytes(SourceText);
                    byte[] signature = SignData(data);

                    Signature = ByteToHex(signature);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand SaveSignature => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] signature = HexToByte(Signature);
                    SaveSignatureWithParameters(signature, SignaturePath);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand Verify => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] data = File.ReadAllBytes(SourceFilePath);
                    byte[] signature = LoadSignatureWithParameters(SignaturePath);
                    bool verified = VerifyData(data, signature);

                    IsVerified = verified;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand BrowseSourcePath => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    OpenFileDialog dialog = new OpenFileDialog()
                    {
                        InitialDirectory = Directory.GetCurrentDirectory()
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        SourceFilePath = dialog.FileName;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand BrowseSignatureSavePath => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    SaveFileDialog dialog = new SaveFileDialog()
                    {
                        InitialDirectory = Directory.GetCurrentDirectory()
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        SignaturePath = dialog.FileName;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        public ICommand BrowseSignatureLoadPath => new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                try
                {
                    OpenFileDialog dialog = new OpenFileDialog()
                    {
                        InitialDirectory = Directory.GetCurrentDirectory()
                    };
                    if (dialog.ShowDialog() == true)
                    {
                        SignaturePath = dialog.FileName;
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            });
        });

        byte[] SignData(byte[] data)
        {
            byte[] hash = sha.ComputeHash(data);
            byte[] signature = dsa.CreateSignature(hash);
            return signature;
        }

        bool VerifyData(byte[] data, byte[] signature)
        {
            byte[] hash = sha.ComputeHash(data);
            return dsa.VerifySignature(hash, signature);
        }

        private string ByteToHex(byte[] data)
        {
            StringBuilder builder = new StringBuilder(data.Length * 2);

            for (int i = 0; i < data.Length; ++i)
            {
                builder.AppendFormat(HexFormat, data[i]);
            }

            return builder.ToString();
        }

        private byte[] HexToByte(string hex)
        {
            byte[] data = new byte[hex.Length / 2];

            for (int i = 0; i < data.Length; ++i)
            {
                string byteValue = hex.Substring(i * 2, 2);
                data[i] = Byte.Parse(byteValue, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            }

            return data;
        }

        private void SaveSignatureWithParameters(byte[] signature, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                string signatureHex = ByteToHex(signature);
                writer.WriteLine(signatureHex);

                string xmlParameters = dsa.ToXmlString(includePrivateParameters: false);
                writer.WriteLine(xmlParameters);
            }
        }

        private byte[] LoadSignatureWithParameters(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string signatureHex = reader.ReadLine();
                byte[] signature = HexToByte(signatureHex);

                string xmlParameters = reader.ReadToEnd();
                dsa.FromXmlString(xmlParameters);

                return signature;
            }
        }
    }
}
