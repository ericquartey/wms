using System;

namespace Ferretto.VW.MAS.Scaffolding.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class IdAttribute : Attribute
    {
        #region Constructors

        public IdAttribute(int id) : this()
        {
            this.Id = id;
        }

        public IdAttribute()
        {
        }

        #endregion

        #region Properties

        public int Id { get; set; }
        
        #endregion
    }
}
