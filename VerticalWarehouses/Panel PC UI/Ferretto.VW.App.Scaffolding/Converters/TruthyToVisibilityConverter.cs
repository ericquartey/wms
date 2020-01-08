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
            bool invert = false;
            string strParam = System.Convert.ToString(parameter, culture);
            if (bool.TryParse(strParam, out bool param))
            {
                invert = param;
            }
            else if ("Invert".Equals(strParam, StringComparison.OrdinalIgnoreCase))
            {
                invert = true;
            }

            // values.
            if (value is System.Collections.IEnumerable enumerable)
            {
                return invert ^ enumerable?.GetEnumerator().MoveNext() == true ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is string text)
            {
                return invert ^ !string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is int count)
            {
                return invert ^ count != default ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is bool boolean)
            {
                return invert ^ boolean ? Visibility.Visible : Visibility.Collapsed;
            }

            return invert ^ value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
