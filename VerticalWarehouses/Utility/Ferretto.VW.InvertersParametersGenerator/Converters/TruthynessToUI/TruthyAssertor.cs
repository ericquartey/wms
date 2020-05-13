using System;
using System.Globalization;
using System.Windows;

namespace Ferretto.VW.InvertersParametersGenerator
{
    internal static class TruthyAssertor
    {
        #region Methods

        public static object Convert(bool value, Type targetType, object parameter)
        {
            if (targetType == typeof(Visibility))
            {
                return value ? Visibility.Visible : Visibility.Collapsed;
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
                return value ? new GridLength(starWeight, GridUnitType.Star) : GridLength.Auto;
            }

            // throw new ArgumentOutOfRangeException($"Type {targetType} is not supported for conversion.");
            return value;
        }

        public static bool IsInverting(object parameter)
        {
            bool invert = false;
            string strParam = System.Convert.ToString(parameter, CultureInfo.InvariantCulture);
            if (bool.TryParse(strParam, out bool param))
            {
                invert = param;
            }
            else if ("Invert".Equals(strParam, StringComparison.OrdinalIgnoreCase))
            {
                invert = true;
            }

            return invert;
        }

        /// <summary>
        /// Returns whether the provided <paramref name="value"/> is kind-of truthy (à la JavaScript).
        /// </summary>
        public static bool IsTruthy(object value)
        {
            if (value is System.Collections.IEnumerable enumerable)
            {
                return enumerable?.GetEnumerator().MoveNext() == true;
            }

            if (value is string text)
            {
                return !string.IsNullOrEmpty(text);
            }

            if (value is int count)
            {
                return count != default;
            }

            if (value is bool boolean)
            {
                return boolean;
            }

            return value != null;
        }

        #endregion
    }
}
