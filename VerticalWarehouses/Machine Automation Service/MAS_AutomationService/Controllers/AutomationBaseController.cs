using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public class AutomationBaseController : ControllerBase
    {
        #region Methods

        protected ActionResult NegativeResult(WMS.Data.WebAPI.Contracts.SwaggerException exception)
        {
            if (exception == null)
            {
                return this.Ok();
            }

            if (exception is WMS.Data.WebAPI.Contracts.SwaggerException<ProblemDetails> problemDetailsException)
            {
                return this.StatusCode(
                    problemDetailsException.StatusCode,
                    problemDetailsException.Result.Title);
            }

            return this.StatusCode(exception.StatusCode, exception.Message);
        }

        protected ActionResult<T> NegativeResult<T>(WMS.Data.WebAPI.Contracts.SwaggerException exception)
            where T : class
        {
            return this.NegativeResult(exception);
        }

        #endregion
    }
}
