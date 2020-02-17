using System;


namespace Ferretto.VW.MAS.DataLayer
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

        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion

        #region Properties

        public string EntityId { get; }

        #endregion
    }
}
