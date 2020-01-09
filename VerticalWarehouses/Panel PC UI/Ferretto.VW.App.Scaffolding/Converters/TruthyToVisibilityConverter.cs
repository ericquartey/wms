using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Ferretto.VW.App.Scaffolding.Converters
{

    /// <summary>
    /// Assumes true or false based on javascript-like truthiness and translates it into <see cref="Visibility"/>, <see cref="GridLength"/>, etc. Should be convenient enough.
    /// </summary>
    public class TruthyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            string strParam = System.Convert.ToString(parameter, culture);
            if (bool.TryParse(strParam, out bool param))
            {
                invert = param;
            }
            else if ("Invert".Equals(strParam, StringComparison.OrdinalIgnoreCase))
            {
                invert = true;
            }

            bool finalValue = invert ^ TruthyAssertor.IsTruthy(value);

            if (targetType == typeof(Visibility))
            {
                return finalValue ? Visibility.Visible : Visibility.Collapsed;
            }
            if (targetType == typeof(GridLength))
            {
                double starWeight = 1D;
                if (parameter != null)
                {
                    var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
                    string stars = System.Convert.ToString(parameter, invariantCulture);
                    if (double.TryParse(stars, NumberStyles.AllowDecimalPoint, invariantCulture, out double actualStars))
                    {
                        starWeight = actualStars;
                    }
                }
                return finalValue ? new GridLength(starWeight, GridUnitType.Star) : GridLength.Auto;
            }

            throw new ArgumentOutOfRangeException($"Type {targetType} is not supported by the {typeof(TruthyToVisibilityConverter)}.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
