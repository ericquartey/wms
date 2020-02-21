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

        public FilterPropertiesAttribute(string propertyName, object value, params string[] propertyList)
        {
            this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
            this.PropertyList = new List<string>(propertyList ?? throw new ArgumentNullException(nameof(propertyList)));
        }

        #endregion

        #region Properties

        public List<string> PropertyList { get; }

        public string PropertyName { get; }

        public object Value { get; }

        #endregion

        #region Methods

        public List<string> GetExclusionList(object item)
        {
            var result = new List<string>();
            if (item != null)
            {
                var currentValue = item.GetType().GetProperty(this.PropertyName).GetValue(item);
                if (currentValue != null && currentValue.Equals(this.Value))
                {
                    return this.PropertyList;
                }
            }
            return result;
        }

        #endregion
    }
}
