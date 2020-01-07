using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple =true)]
    public class CategoryParameterAttribute : Attribute
    {
        public CategoryParameterAttribute(string propertyReference)
        {
            this.PropertyReference = propertyReference;
        }

        public string PropertyReference { get; }

        public Type ValueStringifierType { get; set; }
    }


}
