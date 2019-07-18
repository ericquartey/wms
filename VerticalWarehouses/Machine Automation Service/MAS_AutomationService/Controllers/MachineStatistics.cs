using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/[controller]")]
    [ApiController]
    public class MachineStatisticsController : ControllerBase
    {
        #region Fields

        private readonly IMachineStatisticsDataLayer machineStatisticsDataLayer;

        #endregion

        #region Constructors

        public MachineStatisticsController(IMachineStatisticsDataLayer machineStatisticsDataLayer)
        {
            if (machineStatisticsDataLayer == null)
            {
                throw new ArgumentNullException(nameof(machineStatisticsDataLayer));
            }

            this.machineStatisticsDataLayer = machineStatisticsDataLayer;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<MachineStatistics> Get()
        {
            var statics = this.machineStatisticsDataLayer.GetMachineStatistics();

            return this.Ok(statics);
        }

        #endregion
    }
}
