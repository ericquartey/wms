using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public class BaseWmsProxyController : ControllerBase
    {
        #region Constructors

        protected BaseWmsProxyController()
        {
        }

        #endregion

        #region Methods

        protected ActionResult NegativeResult(WMS.Data.WebAPI.Contracts.WmsWebApiException exception)
        {
            if (exception is null)
            {
                return this.Ok();
            }

            if (exception is WMS.Data.WebAPI.Contracts.WmsWebApiException<ProblemDetails> problemDetailsException)
            {
                return this.StatusCode(
                    problemDetailsException.StatusCode,
                    problemDetailsException.Result);
            }

            return this.StatusCode(exception.StatusCode, exception.Message);
        }

        protected ActionResult<T> NegativeResult<T>(WMS.Data.WebAPI.Contracts.WmsWebApiException exception)
            where T : class
        {
            return this.NegativeResult(exception);
        }

        #endregion
    }
}
