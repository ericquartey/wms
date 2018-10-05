using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.Common.Controls
{
    public class NullFallbackConverter : DependencyObject, IValueConverter
    {
        #region Fields

        private readonly object fallbackValue;

        #endregion Fields

        #region Constructors

        public NullFallbackConverter(object fallbackValue)
        {
            this.fallbackValue = fallbackValue;
        }

        #endregion Constructors

        #region Methods

        public object Convert(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return value ?? this.fallbackValue;
        }

        public object ConvertBack(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion Methods
    }
}
