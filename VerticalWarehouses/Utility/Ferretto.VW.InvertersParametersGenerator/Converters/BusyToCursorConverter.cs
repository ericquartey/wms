using System;

namespace Ferretto.VW.InvertersParametersGenerator
{
    public class BusyToCursorConverter : System.Windows.Data.IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool))
            {
                return System.Windows.Input.Cursors.Arrow;
            }

            var isBusy = (bool)value;

            if (this.Invert)
            {
                isBusy = !isBusy;
            }

            return isBusy ? System.Windows.Input.Cursors.Wait : System.Windows.Input.Cursors.Arrow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
