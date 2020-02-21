using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class FilterPropertiesAttribute : Attribute
    {
        #region Constructors

        public FilterPropertiesAttribute(Type type, string propertyName, object value, params string[] propertyList)
        {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
            this.PropertyList = new List<string>(propertyList ?? throw new ArgumentNullException(nameof(propertyList)));

            this.PropertyInfo = type.GetProperty(propertyName);
        }

        #endregion

        #region Properties

        public List<string> PropertyList { get; }

        public string PropertyName { get; }

        public Type Type { get; }

        public object Value { get; }

        private PropertyInfo PropertyInfo { get; }

        #endregion

        #region Methods

        public List<string> GetExclusionList(object item)
        {
            var currentValue = this.PropertyInfo.GetValue(item);
            var result = new List<string>();

            if (currentValue.Equals(this.Value))
            {
                return this.PropertyList;
            }
            return result;
        }

        #endregion
    }
}
