using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    internal class SuccessOperationResult<T> : IOperationResult<T>
    {
        #region Constructors

        public SuccessOperationResult(T entity)
        {
            this.Entity = entity;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public T Entity { get; }

        public bool Success { get; } = true;

        #endregion
    }
}
