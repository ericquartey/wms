using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.InvertersParametersGenerator
{
    public class ReferenceToVisibilityConverter : DependencyObject, IValueConverter
    {
        #region Fields

        public static readonly DependencyProperty HideProperty = DependencyProperty.Register(
            nameof(Hide),
            typeof(bool),
            typeof(ReferenceToVisibilityConverter));

        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
            nameof(Invert),
            typeof(bool),
            typeof(ReferenceToVisibilityConverter));

        #endregion

        #region Properties

        public bool Hide
        {
            get => (bool)this.GetValue(HideProperty);
            set => this.SetValue(HideProperty, value);
        }

        public bool Invert
        {
            get => (bool)this.GetValue(InvertProperty);
            set => this.SetValue(InvertProperty, value);
        }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException();
            }

            var isValueNull = value == null;

            if (this.Invert)
            {
                isValueNull = !isValueNull;
            }

            if (isValueNull)
            {
                return this.Hide ? Visibility.Hidden : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
