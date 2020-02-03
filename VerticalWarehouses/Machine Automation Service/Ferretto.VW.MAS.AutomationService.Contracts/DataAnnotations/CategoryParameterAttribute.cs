using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class CategoryParameterAttribute : Attribute
    {
        #region Constructors

        public CategoryParameterAttribute(string propertyReference)
        {
            this.PropertyReference = propertyReference;
        }

        #endregion

        #region Properties

        public string PropertyReference { get; }

        public Type ValueStringifierType { get; set; }

        #endregion
    }
}
