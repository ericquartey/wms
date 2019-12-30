using System;

namespace Ferretto.VW.App.Scaffolding.DataAnnotations
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
        public Type TagResourceType { get; set; }
        public string TagResourceName { get; set; }

        public string DefaultValue => this.Tag;
        Type ILocalizableString.ResourceType => this.TagResourceType;
        string ILocalizableString.ResourceName => this.TagResourceName;
    }

}
