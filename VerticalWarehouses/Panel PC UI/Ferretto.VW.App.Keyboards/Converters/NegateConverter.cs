using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Keyboards.Converters
{
    public class NegateConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return -d;
            }
            else if (value is Thickness t)
            {
                return new Thickness(-t.Left, -t.Top, -t.Right, -t.Bottom);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => this.Convert(value, targetType, parameter, culture);

        #endregion
    }
}
