﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Controls
{
    public class StringToVisibilityConverter : DependencyObject, IValueConverter
    {
        #region Fields

        public static readonly DependencyProperty HideProperty = DependencyProperty.Register(
            nameof(Hide),
            typeof(bool),
            typeof(StringToVisibilityConverter));

        public static readonly DependencyProperty InvertProperty = DependencyProperty.Register(
            nameof(Invert),
            typeof(bool),
            typeof(StringToVisibilityConverter));

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
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToVisibilityType);
            }

            var visible = System.Convert.ToString(value, culture)?.Length > 0;

            if ((visible && !this.Invert) || (!visible && this.Invert))
            {
                return Visibility.Visible;
            }

            return this.Hide ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
