using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Vegraris.Wpf.Converters
{
    public class BevelConverter : Drawing.BevelDrawingFactory<StreamGeometry, DrawingContext>, IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<IEnumerable<PiecePoint>> path)
                return Create(path);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
