using Ferretto.VW.App.Scaffolding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class DisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                var display = entity.Metadata?.OfType<DisplayAttribute>().FirstOrDefault();
                if (display != null)
                {
                    return display.GetName();
                }

                value = entity.Property;
            }

            if (value is PropertyInfo prop)
            {
                return prop.DisplayName();
            }

            return System.Convert.ToString(value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
