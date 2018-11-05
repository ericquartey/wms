using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class ReferenceToVisibilityConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public Object Convert(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToVisibilityType);
            }

            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        public Object ConvertBack(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion Methods
    }
}
