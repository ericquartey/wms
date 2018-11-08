using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
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

        #endregion Fields

        #region Properties

        public bool Hide
        {
            get => (bool) this.GetValue(HideProperty);
            set => this.SetValue(HideProperty, value);
        }

        public bool Invert
        {
            get => (bool) this.GetValue(InvertProperty);
            set => this.SetValue(InvertProperty, value);
        }

        #endregion Properties

        #region Methods

        public Object Convert(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToVisibilityType);
            }

            bool val = value == null;

            if (this.Invert)
            {
                val = !val;
            }

            return val ? ( this.Hide ? Visibility.Hidden : Visibility.Collapsed ) : Visibility.Visible;
        }

        public Object ConvertBack(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion Methods
    }
}
