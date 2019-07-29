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

        private readonly IErrorStatisticsProvider errorStatistics;

        #endregion

        #region Constructors

        public ErrorsController(IErrorStatisticsProvider errorStatistics)
        {
            this.errorStatistics = errorStatistics;
        }

        #endregion

        #region Methods

        [HttpGet("Statistics")]
        public ActionResult<ErrorStatisticsSummary> GetStatistics()
        {
            var statistics = this.errorStatistics.GetErrorStatistics();
            return this.Ok(statistics);
        }

        #endregion
    }
}
