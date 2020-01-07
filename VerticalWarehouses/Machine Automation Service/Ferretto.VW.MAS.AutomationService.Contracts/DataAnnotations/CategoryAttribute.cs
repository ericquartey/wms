using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CategoryAttribute : Attribute, ILocalizableString
    {
        public CategoryAttribute()
        {
        }

        public CategoryAttribute(string category)
        {
            this.Category = category;
        }

        public string Category { get; set; }
        public Type ResourceType { get; set; }

        string ILocalizableString.ResourceName => this.Category;
        string ILocalizableString.DefaultValue => this.Category;
    }

}
