using System;
using System.Globalization;
using System.Windows.Data;

namespace lexMD5.WPFApp
{
	class DataModeToIndexConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value is DataMode mode) 
				? (int)mode 
				: -1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Enum.ToObject(typeof(DataMode), value);
		}
	}
}
