using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorsController : BaseAutomationController
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        #endregion

        #region Constructors

        public ErrorsController(
            IErrorsProvider errorsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            if (errorsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(errorsProvider));
            }

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
            var newError = this.errorsProvider.RecordNew(code, BayNumber.None);

            return this.Ok(newError);
        }

#endif

        [HttpPost("{id}/resolve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Error> Resolve(int id)
        {
            var resolvedError = this.errorsProvider.Resolve(id);

            return this.Ok(resolvedError);
        }

        #endregion
    }
}
