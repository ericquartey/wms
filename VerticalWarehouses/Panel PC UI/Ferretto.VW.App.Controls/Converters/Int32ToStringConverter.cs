using System;
using System.Globalization;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class Int32ToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (string)value;
            return string.IsNullOrEmpty(s) ? 0 : (object)int.Parse(s);
        }

        #endregion
    }
}
