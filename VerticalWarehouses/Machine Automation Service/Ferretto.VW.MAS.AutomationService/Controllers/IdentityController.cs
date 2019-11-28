using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitsDataProvider loadingUnitStatisticsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsDataProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineProvider machineProvider,
            IConfiguration configuration,
            WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService)
        {
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.machinesDataService = machinesDataService ?? throw new System.ArgumentNullException(nameof(machinesDataService));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<MachineIdentity> Get()
        {
            var servicingInfo = this.servicingProvider.GetInfo();

            var loadingUnits = this.loadingUnitStatisticsProvider.GetWeightStatistics();

            var machine = this.machineProvider.Get();
            var machineInfo = new MachineIdentity
            {
                AreaId = 1, // TODO remove this hardcoded value
                Id = machine.Id,
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
                TrayCount = loadingUnits.Count(),
                MachineId = 2, // TODO remove this hardcoded value
                MaxGrossWeight = machine.MaxGrossWeight,
                InstallationDate = servicingInfo.InstallationDate,
                NextServiceDate = servicingInfo.NextServiceDate,
                LastServiceDate = servicingInfo.LastServiceDate,
                IsOneTonMachine = this.machineProvider.IsOneTonMachine(),
            };

            return this.Ok(machineInfo);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<MachineStatistics>> GetStatistics([FromServices] IConfiguration configuration)
        {
            var statistics = this.machineProvider.GetStatistics();

            if (configuration.IsWmsEnabled())
            {
                try
                {
                    var machineId = 1; // TODO HACK remove this hardcoded value and use the machine serial number
                    var machine = await this.machinesDataService.GetByIdAsync(machineId);

                    statistics.AreaFillPercentage = machine.AreaFillRate;
                }
                catch (System.Exception)
                {
                    // do nothing:
                    // if the call fails, data from WMS will not be populated
                }
            }

            return this.Ok(statistics);
        }

        #endregion
    }
}
