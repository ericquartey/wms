using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class SuccessOperationResult<T> : IOperationResult<T>
    {
        #region Fields

        private readonly T entity;

        #endregion

        #region Constructors

        public SuccessOperationResult(T entity)
        {
            this.entity = entity;
        }

        #endregion

        #region Properties

        public string Description => throw new System.NotImplementedException();

        public T Entity => this.entity;

        public bool Success => throw new System.NotImplementedException();

        #endregion
    }
}
