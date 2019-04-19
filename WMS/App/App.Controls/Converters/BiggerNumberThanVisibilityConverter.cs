using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.WMS.App.Controls
{
    public class BiggerNumberThanVisibilityConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }

            if (parameter == null)
            {
                return Visibility.Visible;
            }

            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("Errors");
            }

            var fixedParameter = double.Parse(parameter.ToString());

            var doubleValue = double.Parse(value.ToString());
            if (doubleValue < fixedParameter)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
