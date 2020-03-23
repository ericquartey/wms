using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ReferenceToBoolConverter : DependencyObject, IValueConverter
    {
        #region Fields

        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
            nameof(Invert),
            typeof(bool),
            typeof(ReferenceToVisibilityConverter));

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
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException();
            }

            var isValueNull = value == null;

            if (this.Invert)
            {
                isValueNull = !isValueNull;
            }

            return isValueNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
