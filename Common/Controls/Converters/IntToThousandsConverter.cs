using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class IntToThousandsConverter : DependencyObject, IValueConverter
    {
        #region Fields

        private const double Million = Thousand * Thousand;
        private const double Thousand = 1000.0;

        #endregion Fields

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is int == false)
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToStringType);
            }

            var intValue = (int)value;

            if (intValue < Thousand)
            {
                return intValue.ToString(CultureInfo.CurrentUICulture);
            }
            else if (intValue < Million)
            {
                return (intValue / Thousand).ToString($"#.#'{General.ThousandsSymbol}'", CultureInfo.CurrentUICulture);
            }
            else
            {
                return (intValue / Million).ToString($"#.#'{General.MillionsSymbol}'", CultureInfo.CurrentUICulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion Methods
    }
}
