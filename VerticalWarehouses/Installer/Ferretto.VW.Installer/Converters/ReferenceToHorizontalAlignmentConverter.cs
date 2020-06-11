using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.Installer.Converters
{
    public class ReferenceToHorizontalAlignmentConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(HorizontalAlignment))
            {
                throw new InvalidOperationException();
            }

            return value == null ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
