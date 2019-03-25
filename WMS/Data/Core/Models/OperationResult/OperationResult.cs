using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class OperationResult<T> : IOperationResult<T>
    {
        #region Constructors

        protected OperationResult(
             string description,
             T entity = default(T))
             : this(false)
        {
            this.Description = description;
        }

        protected OperationResult(
                    bool success,
            T entity = default(T))
        {
            this.Success = success;
            this.Entity = entity;
        }

        #endregion

        #region Properties

        public string Description { get; set; }

        public T Entity { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
