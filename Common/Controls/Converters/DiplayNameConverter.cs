using System;
using System.Windows.Data;
using Ferretto.Common.Utils.Extensions;

namespace Ferretto.Common.Controls
{
    public class DisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is Type enumType && value is Enum enumValue)
            {
                return enumValue.GetDisplayName(enumType);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
