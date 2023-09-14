using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class CheckDifferenceValueJsonConverter : IMultiValueConverter, IValueConverter
    {
        #region Fields

        private object _entity;

        private PropertyInfo _pinfo;

        #endregion

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //var value = values[0];
            //var entity = values.Length > 1 ? values[1] : parameter;
            //if (value is PropertyInfo pinfo && entity != null)
            //{
            //    // 0. PropertyInfo
            //    // 1. Instance
            //    this._pinfo = pinfo;
            //    this._entity = entity;
            //    var retval = pinfo.GetValue(entity);
            //    if (targetType == typeof(string))
            //    {
            //        return System.Convert.ToString(retval, culture);
            //    }
            //    return retval;
            //}

            //// surrender fallback
            //return values;
            return Color.Red;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //// nullable?
            //if (targetType.IsGenericType && targetType.IsValueType)
            //{
            //    targetType = Nullable.GetUnderlyingType(targetType);
            //}

            return Color.Red;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //if (this._pinfo != null && this._entity != null)
            //{
            //    this._pinfo.SetValue(this._entity, System.Convert.ChangeType(value, this._pinfo.PropertyType, culture));
            //}
            //return new[] { this._pinfo, this._entity };

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return System.Convert.ChangeType(value, targetType, culture);

            return null;
        }

        #endregion
    }
}
