using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Data;

namespace Vegraris.Wpf.Converters
{
    class CastingConverter : IValueConverter
    {
        static class Caster<TSource, TTarget>
        {
            static Caster()
            {
                var sourceType = typeof(TSource);
                var targetType = typeof(TTarget);
                var valueParameter = Expression.Parameter(sourceType);
                Expression convert = valueParameter;
                if (sourceType != targetType)
                    convert = Expression.Convert(valueParameter, targetType);

                func = Expression.Lambda<Func<TSource, TTarget>>(convert, valueParameter).Compile();
            }
            static readonly Func<TSource, TTarget> func;
            public static TTarget Cast(TSource value) => func(value);
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => typeof(Caster<,>).MakeGenericType(value.GetType(), targetType).GetMethod(nameof(Caster<object, object>.Cast))?.Invoke(null, [value]);

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, targetType, parameter, culture);
    }
}