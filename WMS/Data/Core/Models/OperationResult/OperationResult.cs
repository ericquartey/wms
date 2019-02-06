using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class OperationResult<T>
    {
        #region Constructors

        protected OperationResult(
            bool success,
            T entity = default(T))
        {
            this.Success = success;
            this.Entity = entity;
        }

        #endregion

        #region Properties

        public T Entity { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
