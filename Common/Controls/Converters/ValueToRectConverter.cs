using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.Common.Controls
{
    public class ValueToRectConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Rect(0d, 0d, (double)value, (double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
