using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.InvertersParametersGenerator
{
    public class BooleanNegationConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException();
            }

            if (value is bool booleanValue)
            {
                return !booleanValue;
            }

            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
