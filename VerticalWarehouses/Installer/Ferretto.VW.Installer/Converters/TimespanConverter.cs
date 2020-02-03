using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.Installer
{
    internal sealed class TimespanConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.TotalSeconds < 1)
                {
                    return "<1s";
                }

                if (timeSpan.TotalMinutes < 1)
                {
                    return $"{(int)timeSpan.TotalSeconds}s";
                }

                if (timeSpan.TotalHours < 1)
                {
                    return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
                }

                if (timeSpan.TotalDays < 1)
                {
                    return $"{timeSpan.Hours}m {timeSpan.Minutes}m";
                }

                return timeSpan.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
