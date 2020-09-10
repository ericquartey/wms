using System;
using System.Globalization;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class MultiValueConverterAdapter : IMultiValueConverter
    {
        #region Fields

        private object lastParameter;

        #endregion

        #region Properties

        public IValueConverter Converter { get; set; }

        #endregion

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (this.Converter is null)
                {
                    return null;
                }

                if (values.Length > 1)
                {
                    this.lastParameter = values[1];
                }

                return this.Converter.Convert(values[0], targetType, this.lastParameter, culture);
            }
            catch(Exception)
            {
                return values;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            try
            {
                return this.Converter == null
                ? (new object[] { value })
                : (new object[] { this.Converter.ConvertBack(value, targetTypes[0], this.lastParameter, culture) });
            }
            catch(Exception)
            {
                return (object[])value;
            }
        }

        #endregion
    }
}
