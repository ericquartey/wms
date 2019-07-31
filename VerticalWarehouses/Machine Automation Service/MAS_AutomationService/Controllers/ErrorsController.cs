using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorsController : ControllerBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        #endregion

        #region Constructors

        public ErrorsController(IErrorsProvider errorsProvider)
        {
            this.errorsProvider = errorsProvider;
        }

        #endregion

        #region Methods

        [HttpGet("current")]
        public ActionResult<Error> GetCurrent()
        {
            var currentError = this.errorsProvider.GetCurrent();

            return this.Ok(currentError);
        }

        [HttpGet("statistics")]
        public ActionResult<ErrorStatisticsSummary> GetStatistics()
        {
            var statistics = this.errorsProvider.GetStatistics();

            return this.Ok(statistics);
        }

#if DEBUG

        [HttpPost]
        public ActionResult<Error> Create(MachineErrors code)
        {
            var newError = this.errorsProvider.RecordNew(code);

            return this.Ok(newError);
        }

#endif

        [HttpPost("{id}/resolve")]
        public ActionResult<Error> Resolve(int id)
        {
            var resolvedError = this.errorsProvider.Resolve(id);

            if (resolvedError == null)
            {
                return this.NotFound();
            }

            return this.Ok(resolvedError);
        }

        #endregion
    }
}
