﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Controls
{
    public class EnumToBrushConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToBrushType);
            }

            if (!(parameter is Type type))
            {
                throw new InvalidOperationException(Errors.ConverterParameterMustBeType);
            }

            var resourceValue = EnumColors.ResourceManager.GetString($"{type.Name}{value}");
            var color = (Color)(ColorConverter.ConvertFromString(resourceValue) ?? Colors.Transparent);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
