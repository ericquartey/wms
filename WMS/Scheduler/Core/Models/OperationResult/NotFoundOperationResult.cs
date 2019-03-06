using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class NotFoundOperationResult<T> : IOperationResult<T>
    {
        #region Constructors

        public NotFoundOperationResult(string description = null)
        {
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
