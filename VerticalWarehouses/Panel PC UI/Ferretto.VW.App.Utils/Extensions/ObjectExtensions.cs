using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ferretto.VW.Utils.Extensions
{
    public static class ObjectExtensions
    {
        #region Methods

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
            }

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                var value = property.GetValue(source);
                if (value is T)
                {
                    dictionary.Add(property.Name, (T)value);
                }
            }

            return dictionary;
        }

        #endregion
    }
}
