using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class BoolToTextWrappingConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(TextWrapping))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToTextWrappingType);
            }

            var booleanValue = System.Convert.ToBoolean(value, culture);

            return booleanValue ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextWrapping == false)
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToTextWrappingType);
            }

            return (TextWrapping)value != TextWrapping.NoWrap;
        }

        #endregion
    }
}
