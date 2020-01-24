using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CategoryDescriptionAttribute : Attribute, ILocalizableString
    {
        #region Constructors

        public CategoryDescriptionAttribute()
        {
        }

        public CategoryDescriptionAttribute(string description)
        {
            this.Description = description;
        }

        #endregion

        #region Properties

        string ILocalizableString.DefaultValue => this.Description;

        public string Description { get; set; }

        string ILocalizableString.ResourceName => this.Description;

        public Type ResourceType { get; set; }

        #endregion
    }
}
