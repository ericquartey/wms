using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ObjectNullOrEmptyConverter : DependencyObject, IValueConverter
    {
        #region Fields

        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
            nameof(Invert),
            typeof(bool),
            typeof(ObjectNullOrEmptyConverter));

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
            return value == null ? Visibility.Collapsed : string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
