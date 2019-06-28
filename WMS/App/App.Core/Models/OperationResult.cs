using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.WMS.App.Core.Models
{
    public class OperationResult<TModel> : IOperationResult<TModel>
    {
        #region Constructors

        public OperationResult(
            bool success,
            TModel entity = default(TModel),
            bool showToast = true)
        {
            this.Success = success;
            this.Entity = entity;
            this.ShowToast = showToast;
        }

        public OperationResult(System.Exception exception)
        {
            this.Success = false;

            if (exception != null)
            {
                if (exception is SwaggerException<ProblemDetails> swaggerException)
                {
                    this.Description = swaggerException.Result.Detail;
                }
                else
                {
                    this.Description = exception.Message;
                }
            }
        }

        #endregion

        #region Properties

        public string Description { get; private set; }

        public TModel Entity { get; private set; }

        public bool ShowToast { get; private set; }

        public bool Success { get; private set; }

        #endregion
    }
}
