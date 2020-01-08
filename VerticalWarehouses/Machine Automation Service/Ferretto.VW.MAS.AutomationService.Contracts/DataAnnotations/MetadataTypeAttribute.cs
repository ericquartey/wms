using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    /// <summary>
    /// Resolves the idiosincrasy between netcoreapp2.2 and MetadataTypeAttribute.
    /// </summary>
    /// <remarks>Workaround.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false)]
    public class MetadataTypeAttribute : Attribute
    {
        public MetadataTypeAttribute(Type type)
        {
            this.MetadataClassType = type;
        }

        public Type MetadataClassType { get; }
    }

}
