using System;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.BusinessModels
{
    public class OperationResult : IOperationResult
    {
        #region Constructors

        public OperationResult(Exception ex, int? entityId = null)
            : this(false, entityId, ex?.Message)
        {
        }

        public OperationResult(bool success, int? entityId = null, string description = null)
        {
            this.Success = success;
            this.Description = description;
            this.EntityId = entityId;
        }

        #endregion

        #region Properties

        public string Description { get; private set; }

        public int? EntityId { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
