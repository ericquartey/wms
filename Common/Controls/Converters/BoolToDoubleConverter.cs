using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class BoolToDoubleConverter : DependencyObject, IValueConverter
    {
        #region Properties

        public object ValueOnFalse { get; set; }

        public object ValueOnTrue { get; set; }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToDoubleType);
            }

            return System.Convert.ToBoolean(value, culture)
                ? System.Convert.ToDouble(this.ValueOnTrue, culture)
                : System.Convert.ToDouble(this.ValueOnFalse, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
