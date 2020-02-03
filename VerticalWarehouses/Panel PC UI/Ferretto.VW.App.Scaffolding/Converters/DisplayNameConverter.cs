using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class DisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                return entity.DisplayName();
            }

            if (value is PropertyInfo prop)
            {
                return prop.Name;
            }

            return System.Convert.ToString(value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
