using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Vegraris.Wpf.Converters
{
    public class NumericProductConverter : IValueConverter, IMultiValueConverter
    {
        private readonly static LengthConverter doubleConverter = new();

        public double Default { get; set; } = 1;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = Default;
            foreach (var value in values)
                if (doubleConverter.IsValid(value))
                    result *= (double?)doubleConverter.ConvertFrom(value) ?? 1d;
            return result;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
             => Default * (doubleConverter.IsValid(value) ? (double?)doubleConverter.ConvertFrom(value) ?? 1d : 1d);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}