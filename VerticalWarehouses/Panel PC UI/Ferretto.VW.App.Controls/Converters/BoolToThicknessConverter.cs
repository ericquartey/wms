using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BoolToThicknessConverter : DependencyObject, IValueConverter
    {
        #region Properties

        public object ValueOnFalse { get; set; } = 0.0;

        public object ValueOnTrue { get; set; } = 1.0;

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Thickness))
            {
                throw new InvalidOperationException();
            }

            return System.Convert.ToBoolean(value, culture)
                ? new Thickness(System.Convert.ToDouble(this.ValueOnTrue, culture))
                : new Thickness(System.Convert.ToDouble(this.ValueOnFalse, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
