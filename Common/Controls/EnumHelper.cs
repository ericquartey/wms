using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls
{
    public static class EnumHelper
    {
        #region Methods

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static string GetDisplay(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            DisplayAttribute[] attributes = (DisplayAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length != 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            string resourceId = attributes[0].Name;
            Type type = typeof(Enum);
            PropertyInfo nameProperty = type.GetProperty(resourceId, BindingFlags.Static | BindingFlags.Public);
            object result = nameProperty.GetValue(nameProperty.DeclaringType, null);

            return (result != null) ? result.ToString() : string.Empty;
        }

        public static Dictionary<string, string> GetDisplayValues(this Type enumType)
        {
            var enumValues = new Dictionary<string, string>();

            foreach (Enum value in Enum.GetValues(enumType))
            {
                enumValues.Add(value.ToString(), GetDisplay(value));
            }
            return enumValues;
        }

        #endregion Methods
    }
}
