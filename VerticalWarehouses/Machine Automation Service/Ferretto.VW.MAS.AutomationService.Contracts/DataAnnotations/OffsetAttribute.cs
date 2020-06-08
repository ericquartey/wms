using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class OffsetAttribute : Attribute
    {
        #region Constructors

        public OffsetAttribute(int offset) : this()
        {
            this.Offset = offset;
        }

        public OffsetAttribute()
        {
        }

        #endregion

        #region Properties

        public int Offset { get; set; }

        #endregion
    }
}
