using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.App.Core.Models
{
    public class OperationResult<TModel> : IOperationResult<TModel>
    {
        #region Constructors

        public OperationResult(
            bool success,
            TModel entity = default(TModel))
        {
            this.Success = success;
            this.Entity = entity;
        }

        public OperationResult(
          System.Exception exception)
        {
            this.Success = false;
            this.Description = exception?.Message;
        }

        #endregion

        #region Properties

        public string Description { get; private set; }

        public TModel Entity { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
