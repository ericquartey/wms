using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
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

        private readonly IMachineStatisticsProvider machineStatisticsProvider;

        #endregion

        #region Constructors

        public StatisticsController(
            IMachineStatisticsProvider machineStatisticsProvider,
            IMachinesDataService machinesDataService)
        {
            if (machineStatisticsProvider is null)
            {
                throw new ArgumentNullException(nameof(machineStatisticsProvider));
            }

            if (machinesDataService is null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            this.machineStatisticsProvider = machineStatisticsProvider;
            this.machinesDataService = machinesDataService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<MachineStatistics>> GetAsync()
        {
            var statics = this.machineStatisticsProvider.GetMachineStatistics();

            try
            {
                var machineId = 1; // TODO get WMS machine id
                var machine = await this.machinesDataService.GetByIdAsync(machineId);

                statics.AreaFillPercentage = machine.AreaFillRate;
            }
            catch (Exception)
            {
                // do nothing:
                // if the call fails, some data will not be populated
            }

            return this.Ok(statics);
        }

        #endregion
    }
}
