using System;
using System.Reflection;

namespace Ferretto.Common.Utils.Extensions
{
    public static class PropertyInfoExtensions
    {
        #region Methods

        public static bool HasEmptyValue(this PropertyInfo propertyInfo, object instance)
        {
            if (propertyInfo == null)
            {
                return true;
            }

            var propertyType = propertyInfo.PropertyType;
            var propertyValue = propertyInfo.GetValue(instance);
            if (propertyType.IsEnum)
            {
                return propertyValue == null || (int)propertyValue == 0;
            }

            if (propertyType == typeof(DateTime))
            {
                return propertyValue == null || (DateTime)propertyValue == DateTime.MinValue;
            }

            return propertyValue == null;
        }

        #endregion
    }
}
