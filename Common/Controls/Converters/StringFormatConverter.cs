using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class StringFormatConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToDoubleType);
            }
            return string.Format(values[0].ToString(), values[1], values[2]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
