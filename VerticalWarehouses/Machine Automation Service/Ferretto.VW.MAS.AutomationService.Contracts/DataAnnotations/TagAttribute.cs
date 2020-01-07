using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TagAttribute : Attribute, ILocalizableString
    {
        public TagAttribute()
        {
        }

        public TagAttribute(string tag)
        {
            this.Tag = tag;
        }

        public string Tag { get; set; }
        public Type ResourceType { get; set; }

        string ILocalizableString.DefaultValue => this.Tag;
        string ILocalizableString.ResourceName => this.Tag;
    }

}
