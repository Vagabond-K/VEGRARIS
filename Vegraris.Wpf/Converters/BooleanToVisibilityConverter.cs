using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Vegraris.Wpf.Converters
{
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool Invert { get; set; }
        public bool HiddenInsteadOfCollapsed { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || value == Binding.DoNothing) return value;

            return (bool)System.Convert.ChangeType(value, typeof(bool)) != Invert ? Visibility.Visible : (HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
