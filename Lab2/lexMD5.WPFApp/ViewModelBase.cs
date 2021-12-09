using System;
using System.ComponentModel;

namespace lexMD5.WPFApp
{
	internal class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _errorMessage;
		public string ErrorMessage
		{
			get => _errorMessage;
			set
			{
				_errorMessage = value;
				RaisePropertyChanged(nameof(ErrorMessage));
			}
		}

		protected void ResetError()
		{
			ErrorMessage = String.Empty;
		}
	}
}
