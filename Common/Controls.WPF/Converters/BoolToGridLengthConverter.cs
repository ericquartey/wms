using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.Common.Controls.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class BoolToGridLengthConverter : DependencyObject, IValueConverter
    {
        #region Fields

        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
                                nameof(Invert),
                                typeof(bool),
                                typeof(BoolToGridLengthConverter));

        #endregion

        #region Properties

        public bool Invert
        {
            get => (bool)this.GetValue(InvertProperty);
            set => this.SetValue(InvertProperty, value);
        }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var setLenght = (bool)value;
            if (this.Invert)
            {
                setLenght = !setLenght;
            }

            if (setLenght && parameter is string param &&
                double.TryParse(param, out var valueParam))
            {
                return new GridLength(value: valueParam, type: GridUnitType.Pixel);
            }

            return setLenght ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
