using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DefaultValueAttribute : Attribute
    {
        #region Constructors

        public DefaultValueAttribute(object value)
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        #endregion

        #region Properties

        public object Value { get; }

        #endregion
    }
}
