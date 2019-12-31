using System;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DefaultValueAttribute : Attribute
    {
        public DefaultValueAttribute(object value) : this(value, default)
        {
        }

        public DefaultValueAttribute(object value, string unit)
        {
            this.Unit = unit;
            this.Value = value;
        }

        public string Unit { get; }
        public object Value { get; }
    }

}
