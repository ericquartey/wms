using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class ErrorsController : ControllerBase
    {
        #region Fields

        private readonly IErrorStatisticsDataLayer errorStatistics;

        #endregion

        #region Constructors

        public ErrorsController(IErrorStatisticsDataLayer errorStatistics)
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
