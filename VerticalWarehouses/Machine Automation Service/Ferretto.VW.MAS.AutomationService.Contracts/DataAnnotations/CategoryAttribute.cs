using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CategoryAttribute : Attribute, ILocalizableString
    {
        #region Constructors

        public CategoryAttribute()
        {
        }

        public CategoryAttribute(string category)
        {
            this.Category = category;
        }

        #endregion

        #region Properties

        public string Category { get; set; }

        string ILocalizableString.DefaultValue => this.Category;

        string ILocalizableString.ResourceName => this.Category;

        public Type ResourceType { get; set; }

        #endregion
    }
}
