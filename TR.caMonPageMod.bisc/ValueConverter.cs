using System;
using System.Globalization;
using System.Windows.Data;

namespace TR.caMonPageMod.bisc
{
	public class IntToString : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> ((int?)value ?? 0).ToString();

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s)
			{
				if (string.IsNullOrWhiteSpace(s))
					return 0;

				return int.Parse(s);
			}
			else
				throw new NotSupportedException();
		}
	}
	public class DoubleToString : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return ((double?)value).ToString();
			}
			catch (InvalidCastException)
			{
				try
				{
					return ((int?)value).ToString();
				}
				catch (Exception)
				{
					throw;
				}
			}
			catch(Exception)
			{
				throw;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string s)
			{
				if (string.IsNullOrWhiteSpace(s))
					return double.NaN;

				return double.Parse(s);
			}
			else
				throw new NotSupportedException();
		}
	}

	public class NullableBoolToBool : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (bool?)value ?? false;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> value;
	}

	public class Bool_TFInv : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> !(bool)value;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> !(bool)value;
	}

	public class IsNotNULL : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is not null;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
