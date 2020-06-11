using System;
using System.Globalization;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class DoubleNullableToProgressValueConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? (double)0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
