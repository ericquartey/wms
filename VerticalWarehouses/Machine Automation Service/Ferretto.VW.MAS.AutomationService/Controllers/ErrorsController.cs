using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable ArrangeThisQualifier
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
            this.errorsProvider = errorsProvider ?? throw new System.ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Methods

        [HttpGet("current")]
        public ActionResult<MachineError> GetCurrent()
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

        [HttpPost("{id}/resolve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<MachineError> Resolve(int id)
        {
            var resolvedError = this.errorsProvider.Resolve(id);

            return this.Ok(resolvedError);
        }

        [HttpPost("resolveall")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<MachineError> ResolveAll()
        {
            this.errorsProvider.ResolveAll();
            return this.Ok();
        }

        #endregion
    }
}
