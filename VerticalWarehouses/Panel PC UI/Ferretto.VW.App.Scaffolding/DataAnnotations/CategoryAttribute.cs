using System;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
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
        public Type CategoryResourceType { get; set; }
        public string CategoryResourceName { get; set; }

        Type ILocalizableString.ResourceType => this.CategoryResourceType;
        string ILocalizableString.ResourceName => this.CategoryResourceName;
        string ILocalizableString.DefaultValue => this.Category;
    }

}
