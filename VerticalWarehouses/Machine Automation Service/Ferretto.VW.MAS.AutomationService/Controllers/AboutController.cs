using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        #region Fields

        private readonly IStatisticsDataProvider statisticsProvider;

        #endregion

        #region Constructors

        public AboutController(IStatisticsDataProvider statisticsProvider)
        {
            this.statisticsProvider = statisticsProvider ?? throw new ArgumentNullException(nameof(statisticsProvider));
        }

        #endregion

        #region Methods

        [HttpGet("missiontotalnumber")]
        public ActionResult<int> MissionTotalNumber()
        {
            return this.Ok(this.statisticsProvider.MissionTotalNumber());
        }

        #endregion
    }
}
