using System;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        #region Constructors

        public EntityNotFoundException(int entityId)
            : base(string.Format(Resources.General.NoEntityWithTheSpecifiedIdExists, entityId))
        {
            this.EntityId = entityId.ToString();
        }

        public EntityNotFoundException(string entityId)
            : base(string.Format(Resources.General.NoEntityWithTheSpecifiedIdExists, entityId))
        {
            this.EntityId = entityId;
        }

        #endregion

        #region Properties

        public string EntityId { get; }

        #endregion
    }
}
