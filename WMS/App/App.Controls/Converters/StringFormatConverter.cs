using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Controls
{
    public class StringFormatConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToDoubleType);
            }

            for (var i = 1; i < values.Length; i++)
            {
                if (string.IsNullOrWhiteSpace($"{values[i]}"))
                {
                    return string.Empty;
                }
            }

            return string.Format(values[0].ToString(), values.Skip(1).ToArray());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
