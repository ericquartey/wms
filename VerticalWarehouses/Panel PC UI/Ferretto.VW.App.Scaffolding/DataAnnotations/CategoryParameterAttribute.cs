using System;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple =true)]
    public class CategoryParameterAttribute : Attribute
    {
        public CategoryParameterAttribute(string propertyReference)
        {
            this.PropertyReference = propertyReference;
        }

        public string PropertyReference { get; }
    }
}
