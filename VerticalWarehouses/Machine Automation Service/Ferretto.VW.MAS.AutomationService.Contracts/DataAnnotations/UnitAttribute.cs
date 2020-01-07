using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UnitAttribute : Attribute, ILocalizableString
    {
        public UnitAttribute(string unit) : this()
        {
            this.Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        }

        public UnitAttribute()
        {
        }

        public string Unit { get; set; }

        public Type ResourceType { get; set; }

        string ILocalizableString.ResourceName => this.Unit;

        string ILocalizableString.DefaultValue => this.Unit;
    }
    
}
