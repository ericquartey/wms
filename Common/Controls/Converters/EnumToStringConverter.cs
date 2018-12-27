using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Ferretto.Common.BusinessModels;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Ferretto.Common.Controls
{
    [ValueConversion(typeof(object), typeof(string))]
    public class EnumToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var x = value.ToString();
                var field = value.GetType().GetField(value.ToString());

                //if (field != null)
                //{
                //    var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);

                //    return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
                //}
                //return value;

                //return ResourceManager.GetString(value.ToString());

                if (!(parameter is Type type))
                {
                    //throw new InvalidOperationException(Errors.ConverterParameterMustBeType);
                }
                //var resourceValue = EnumColors.ResourceManager.GetString($"{type.Name}{value}");
                //var color = (Color)(ColorConverter.ConvertFromString(resourceValue) ?? Colors.Transparent);

                //return new SolidColorBrush(color);
            }

            if (value is EntityQueryable<EnumerationString> x)
            {
                var attr = EnumHelper.GetAttributeOfType<string>(x);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion Methods
    }
}
