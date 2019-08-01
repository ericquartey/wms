using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        #region Fields

        private readonly IMachinesDataService machinesDataService;

        private readonly IMachineStatisticsDataLayer machineStatisticsDataLayer;

        #endregion

        #region Constructors

        public StatisticsController(
            IMachineStatisticsDataLayer machineStatisticsDataLayer,
            IMachinesDataService machinesDataService)
        {
            if (machineStatisticsDataLayer == null)
            {
                throw new ArgumentNullException(nameof(machineStatisticsDataLayer));
            }

            if (machinesDataService == null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            this.machineStatisticsDataLayer = machineStatisticsDataLayer;
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<MachineStatistics>> GetAsync()
        {
            var statics = this.machineStatisticsDataLayer.GetMachineStatistics();

            try
            {
                // TODO get machine id
                var machineId = 1;
                var machine = await this.machinesDataService.GetByIdAsync(machineId);

                statics.AreaFillPercentage = machine.AreaFillRate;
            }
            catch (System.Exception)
            {
                // do nothing:
                // if the call fails, some data will not be populated
            }

            return this.Ok(statics);
        }

        #endregion
    }
}
