using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    /// <summary>
    /// Placeholder attribute, that can be associated with a <see cref="CategoryAttribute"/>. When present, pulls the relevant property (or category) up to the root of the metadata tree.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PullToRootAttribute : Attribute
    {
    }

    /// <summary>
    /// Gives a numeric id to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PropertyIdAttribute : Attribute
    {
        public PropertyIdAttribute(int id)
        {
            this.Id = id;
        }

        public int Id { get; }
    }
}
