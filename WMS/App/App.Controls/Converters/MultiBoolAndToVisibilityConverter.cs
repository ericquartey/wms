using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Controls
{
    public class MultiBoolAndToVisibilityConverter : DependencyObject, IMultiValueConverter
    {
        #region Fields

        public static readonly DependencyProperty HideProperty = DependencyProperty.Register(
            nameof(Hide),
            typeof(bool),
            typeof(MultiBoolAndToVisibilityConverter));

        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
            nameof(Invert),
            typeof(bool),
            typeof(MultiBoolAndToVisibilityConverter));

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

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToVisibilityType);
            }

            var visible = values.OfType<bool>().All(x => x);

            if ((visible && !this.Invert) || (!visible && this.Invert))
            {
                return Visibility.Visible;
            }

            return this.Hide ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
