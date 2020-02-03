using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    /// <summary>
    /// Resolves the idiosincrasy between netcoreapp2.2 and MetadataTypeAttribute.
    /// </summary>
    /// <remarks>Workaround.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        #region Constructors

        public MetadataTypeAttribute(Type type)
        {
            this.MetadataClassType = type;
        }

        #endregion

        #region Properties

        public Type MetadataClassType { get; }

        #endregion
    }
}
