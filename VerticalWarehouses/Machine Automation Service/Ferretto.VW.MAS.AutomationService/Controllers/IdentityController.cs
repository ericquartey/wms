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

        private readonly ILoadingUnitsProvider loadingUnitStatisticsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineProvider machineProvider,
            WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService)
        {
            if (loadingUnitStatisticsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            }

            if (servicingProvider is null)
            {
                throw new System.ArgumentNullException(nameof(servicingProvider));
            }

            if (machineProvider is null)
            {
                throw new System.ArgumentNullException(nameof(machineProvider));
            }

            if (machinesDataService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesDataService));
            }

            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider;
            this.servicingProvider = servicingProvider;
            this.machineProvider = machineProvider;
            this.machinesDataService = machinesDataService;
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
                AreaId = 2, // TODO remove this hardcoded value
                Width = 3080, // TODO remove this hardcoded value
                Depth = 500, // TODO remove this hardcoded value
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
                TrayCount = loadingUnits.Count(),
                MachineId = 1, // TODO remove this hardcoded value
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
