﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultOpacity = 0.5d;

            if (parameter is string opacityParam)
            {
                if (double.TryParse(opacityParam, out var newOpacity))
                {
                    defaultOpacity = newOpacity;
                }
            }

            return (bool)value ? defaultOpacity : 1d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
