using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Methods

        protected ObjectResult NegativeResponse<T>(IOperationResult<T> operationResult)
        {
            if (operationResult == null)
            {
                throw new System.ArgumentNullException(nameof(operationResult));
            }

            if (operationResult.Success)
            {
                throw new System.InvalidOperationException();
            }

            switch (operationResult)
            {
                case UnprocessableEntityOperationResult<T> result:
                    {
                        return this.UnprocessableEntity(new ProblemDetails
                        {
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = result?.Description
                        });
                    }

                case BadRequestOperationResult<T> result:
                    {
                        return this.BadRequest(new ProblemDetails
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Detail = result?.Description
                        });
                    }

                case NotFoundOperationResult<T> result:
                    {
                        return this.NotFound(new ProblemDetails
                        {
                            Status = StatusCodes.Status404NotFound,
                            Detail = result?.Description
                        });
                    }

                case CreationErrorOperationResult<T> result:
                    {
                        return this.UnprocessableEntity(new ProblemDetails
                        {
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = result?.Description
                        });
                    }

                default:
                    throw new System.InvalidOperationException();
            }
        }

        #endregion
    }
}
