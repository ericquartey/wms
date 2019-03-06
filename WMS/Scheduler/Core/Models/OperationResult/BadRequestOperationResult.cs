using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class BadRequestOperationResult<T> : IOperationResult<T>
    {
        #region Constructors

        public BadRequestOperationResult(T entity, string description = null)
        {
            this.Entity = entity;
            this.Description = description;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public T Entity { get; }

        public bool Success { get; }

        #endregion
    }
}
