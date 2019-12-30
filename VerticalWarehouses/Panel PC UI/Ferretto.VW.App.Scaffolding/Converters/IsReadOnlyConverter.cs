using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class IsReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                // detour to the 'PropertyInfo'
                value = entity.Property;
            }
            
            if (value is PropertyInfo prop)
            {
                var editable = prop.GetCustomAttribute<EditableAttribute>();
                if (editable != null)
                {
                    return !editable.AllowEdit;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
