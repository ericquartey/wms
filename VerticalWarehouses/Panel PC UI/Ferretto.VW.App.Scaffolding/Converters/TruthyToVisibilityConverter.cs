using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Ferretto.VW.App.Scaffolding.Converters
{

    /// <summary>
    /// Assumes true or false based on javascript-like truthiness and translates it into <see cref="Visibility"/>, <see cref="GridLength"/>, bool, etc. Should be convenient enough.
    /// </summary>
    public class TruthyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = TruthyAssertor.IsInverting(parameter);
            bool finalValue = invert ^ TruthyAssertor.IsTruthy(value);

            return TruthyAssertor.Convert(finalValue, targetType, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
