using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class MultiBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool finalValue = true;
            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value.Equals(false))
                    {
                        finalValue = false;
                        break;
                    }
                }
            }

            return TruthyAssertor.Convert(finalValue, targetType, parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
