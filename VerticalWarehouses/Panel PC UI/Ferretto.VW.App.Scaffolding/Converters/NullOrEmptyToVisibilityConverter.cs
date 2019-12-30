using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.IEnumerable enumerable)
            {
                return enumerable?.GetEnumerator().MoveNext() == true ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is string text)
            {
                return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (value is int count)
            {
                return count == default ? Visibility.Collapsed : Visibility.Visible;
            }

            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
