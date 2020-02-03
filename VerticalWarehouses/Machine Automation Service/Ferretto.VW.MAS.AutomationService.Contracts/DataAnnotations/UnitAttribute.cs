using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class UnitAttribute : Attribute, ILocalizableString
    {
        #region Constructors

        public UnitAttribute(string unit) : this()
        {
            this.Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        }

        public UnitAttribute()
        {
        }

        #endregion

        #region Properties

        string ILocalizableString.DefaultValue => this.Unit;

        string ILocalizableString.ResourceName => this.Unit;

        public Type ResourceType { get; set; }

        public string Unit { get; set; }

        #endregion
    }
}
