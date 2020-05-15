using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class DefaultValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                value = entity.Metadata;
            }

            if (value is IEnumerable<Attribute> metadata)
            {
                var output = System.Convert.ToString(parameter, culture)?.ToLowerInvariant();
                switch (output)
                {
                    case "min":
                        return metadata.RangeMin();
                    case "max":
                        return metadata.RangeMax();

                    case "unit":
                        return metadata.UnitOfMeasure();
                    default:
                        return metadata.DefaultValue();
                }

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
