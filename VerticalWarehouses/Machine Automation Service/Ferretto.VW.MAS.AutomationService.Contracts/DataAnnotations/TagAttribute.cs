using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TagAttribute : Attribute, ILocalizableString
    {
        #region Constructors

        public TagAttribute()
        {
        }

        public TagAttribute(string tag)
        {
            this.Tag = tag;
        }

        #endregion

        #region Properties

        string ILocalizableString.DefaultValue => this.Tag;

        string ILocalizableString.ResourceName => this.Tag;

        public Type ResourceType { get; set; }

        public string Tag { get; set; }

        #endregion
    }
}
