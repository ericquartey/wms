using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        #region Fields

        private readonly IStatisticsDataProvider statisticsProvider;

        #endregion

        #region Constructors

        public StatisticsController(
            IStatisticsDataProvider statisticsProvider)
        {
            this.statisticsProvider = statisticsProvider ?? throw new System.ArgumentNullException(nameof(statisticsProvider));
        }

        #endregion

        #region Methods

        [HttpPost("confirm-statistics")]
        public ActionResult<int> ConfirmStatistics()
        {
            return this.Ok(this.statisticsProvider.ConfirmAndCreateNew());
        }

        [HttpGet("actual-statistics")]
        public ActionResult<MachineStatistics> GetActual()
        {
            return this.Ok(this.statisticsProvider.GetActual());
        }

        [HttpGet("all-statistics")]
        public ActionResult<IEnumerable<MachineStatistics>> GetAll()
        {
            return this.Ok(this.statisticsProvider.GetAll());
        }

        [HttpGet("actual-statistics")]
        public ActionResult<MachineStatistics> GetById(int id)
        {
            return this.Ok(this.statisticsProvider.GetById(id));
        }

        [HttpGet("last-confirmed-statistics")]
        public ActionResult<MachineStatistics> GetLastConfirmed()
        {
            return this.Ok(this.statisticsProvider.GetLastConfirmed());
        }

        #endregion
    }
}
