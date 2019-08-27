using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataLayer.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        #region Constructors

        public EntityNotFoundException(int entityId)
            : base($"No entity with the specified id '{entityId}' exists.")
        {
            this.EntityId = entityId;
        }

        #endregion

        #region Properties

        public int EntityId { get; }

        #endregion
    }
}
