using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{

    public class EnumDatasourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                Type enumType = entity.Metadata.OfType<EnumDataTypeAttribute>().Select(a => a.EnumType).FirstOrDefault() ?? entity.Property.PropertyType;
                // permissive behavior
                if (enumType.IsEnum)
                {
                    return Enum.GetValues(enumType); //.Cast<object>().Select(o => new { Value = o, EnumType = enumType });
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
