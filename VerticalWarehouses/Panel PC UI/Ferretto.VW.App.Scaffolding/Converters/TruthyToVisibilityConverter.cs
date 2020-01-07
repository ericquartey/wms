using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    /// <summary>
    /// Assumes true or false based on javascript-like truthiness and translates it into <see cref="Visibility"/>. Should be convenient enough.
    /// </summary>
    public class TruthyToVisibilityConverter : IValueConverter
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

            if (value is bool boolean)
            {
                return boolean ? Visibility.Visible : Visibility.Collapsed;
            }

            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
