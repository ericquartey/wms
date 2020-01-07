using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DefaultValueAttribute : Attribute
    {
        public DefaultValueAttribute(object value) 
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public object Value { get; }
    }

}
