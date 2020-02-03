using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class TwoWayConverter : IMultiValueConverter, IValueConverter
    {
        private PropertyInfo _pinfo;
        private object _entity;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object value = values[0];
            object entity = values.Length > 1 ? values[1] : parameter;
            if (value is PropertyInfo pinfo && entity != null)
            {
                // 0. PropertyInfo
                // 1. Instance
                this._pinfo = pinfo;
                this._entity = entity;
                object retval = pinfo.GetValue(entity);
                if (targetType == typeof(string))
                {
                    return System.Convert.ToString(retval, culture);
                }
                return retval;
            }

            // surrender fallback
            return values;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ChangeType(value, targetType, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (this._pinfo != null && this._entity != null)
            {
                this._pinfo.SetValue(this._entity, System.Convert.ChangeType(value, this._pinfo.PropertyType, culture));
            }
            return new[] { this._pinfo, this._entity };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ChangeType(value, targetType, culture);
        }
    }
}
