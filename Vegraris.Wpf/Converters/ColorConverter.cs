using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Vegraris.Palettes;

namespace Vegraris.Wpf.Converters
{
    public class ColorConverter : ColorConverter<Color>, IValueConverter
    {
        protected override Color ToColor(uint color)
            => Color.FromArgb(
                (byte)(color >> 24 & 0xFF),
                (byte)(color >> 16 & 0xFF),
                (byte)(color >> 8 & 0xFF),
                (byte)(color & 0xFF));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Tetromino tetromino)
                return ToColor(tetromino);
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}