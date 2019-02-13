using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Scheduler.Core.Providers
{
    internal class SuccessOperationResult<T> : IOperationResult<T>
    {
        #region Fields

        private readonly T model;

        #endregion

        #region Constructors

        public SuccessOperationResult(T model)
        {
            this.model = model;
        }

        #endregion

        #region Properties

        public T Model => this.model;

        #endregion
    }
}
