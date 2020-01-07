using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    /// <summary>
    /// Placeholder attribute that defines whether the properties of the target type should be unfolded in a plain list, or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UnfoldAttribute : Attribute
    {
    }
}
