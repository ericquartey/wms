using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Grid;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Controls
{
    public class BoolToNavigationStyleConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public Object Convert(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType != typeof(GridViewNavigationStyle))
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToNavigationStyleType);
            }

            var enable = System.Convert.ToBoolean(value, culture);

            return enable ? GridViewNavigationStyle.Row : GridViewNavigationStyle.None;
        }

        public Object ConvertBack(object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value is GridViewNavigationStyle == false)
            {
                throw new InvalidOperationException(Errors.ConverterCanConvertOnlyToNavigationStyleType);
            }

            var enable = ((GridViewNavigationStyle)value == GridViewNavigationStyle.Row ? true : false);

            return enable;
        }

        #endregion Methods
    }
}
