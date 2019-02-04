using System;
using System.Windows.Data;

namespace Ferretto.Common.Controls
{
    public class XyToPointConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(values[0] is double &&
                  values[1] is double))
            {
                return null;
            }

            var xValue = (double)values[0];
            var yValue = (double)values[1];
            return new System.Windows.Point(xValue, yValue);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
