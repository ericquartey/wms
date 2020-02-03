using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class IsEditableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                // detour to the 'Metadata'
                value = entity.Metadata;
            }

            if (value is IEnumerable<Attribute> metadata)
            {
                var editable = metadata.OfType<EditableAttribute>().FirstOrDefault();
                if (editable != null)
                {
                    return editable.AllowEdit;
                }
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
