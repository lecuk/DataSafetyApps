using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace lexDSS
{
	internal class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
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
