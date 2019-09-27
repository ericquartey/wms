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

        private readonly IMachineProvider machineProvider;

        private readonly IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public StatisticsController(
            IMachineProvider machineProvider,
            IMachinesDataService machinesDataService)
        {
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
        }

        #endregion

        #region Properties

        public IMachineProvider MachineStatisticsProvider => this.machineProvider;

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<MachineStatistics>> GetAsync()
        {
            var statics = this.MachineStatisticsProvider.GetStatistics();

            try
            {
                var machineId = 1; // TODO HACK remove this hardcoded value
                var machine = await this.machinesDataService.GetByIdAsync(machineId);

                statics.AreaFillPercentage = machine.AreaFillRate;
            }
            catch (Exception)
            {
                // do nothing:
                // if the call fails, data from WMS will not be populated
            }

            return this.Ok(statics);
        }

        #endregion
    }
}
