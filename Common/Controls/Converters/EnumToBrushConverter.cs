using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class EnumToBrushConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public Object Convert(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToBrushType);
            }

            if (!( parameter is Type type ))
            {
                throw new InvalidOperationException(Errors.ConverterParameterMustBeType);
            }
            var resourceValue = EnumColors.ResourceManager.GetString($"{type.Name}{value}");
            var color = (Color) (ColorConverter.ConvertFromString(resourceValue) ?? Colors.Transparent);

            return new SolidColorBrush(color);
        }

        public Object ConvertBack(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion Methods
    }
}
