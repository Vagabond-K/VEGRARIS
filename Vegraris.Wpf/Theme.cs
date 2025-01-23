using System.Collections.ObjectModel;
using System.Windows;

namespace Vegraris.Wpf
{
    public class Theme
    {
        public static string GetSource(FrameworkElement obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(FrameworkElement obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string), typeof(Theme), new PropertyMetadata(null, OnSourceChanged));

        class ThemeResourceDictionary : ResourceDictionary
        {
            public Collection<ResourceDictionary>? owner;
        }

        private static readonly DependencyProperty ThemeResourceDictionaryProperty =
            DependencyProperty.RegisterAttached("ThemeResourceDictionary", typeof(ThemeResourceDictionary), typeof(Theme), new PropertyMetadata(null));

        private static void OnSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is FrameworkElement element && element.Resources != null)
            {
                if (element.GetValue(ThemeResourceDictionaryProperty) is ThemeResourceDictionary oldTheme)
                    oldTheme.owner?.Remove(oldTheme);

                if (e.NewValue is string source)
                {
                    var newTheme = new ThemeResourceDictionary()
                    {
                        Source = new Uri(source, UriKind.RelativeOrAbsolute),
                        owner = element.Resources.MergedDictionaries
                    };
                    newTheme.owner.Add(newTheme);
                    element.SetValue(ThemeResourceDictionaryProperty, newTheme);
                }
            }
        }
    }
}
