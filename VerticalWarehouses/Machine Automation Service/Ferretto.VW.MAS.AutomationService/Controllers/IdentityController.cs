using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly ILoadingUnitsDataProvider loadingUnitStatisticsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMachinesWmsWebService machinesWmsWebService;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsDataProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineProvider machineProvider,
            IConfiguration configuration,
            IMachinesWmsWebService machinesWmsWebService)
        {
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            this.machinesWmsWebService = machinesWmsWebService ?? throw new System.ArgumentNullException(nameof(machinesWmsWebService));
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<MachineIdentity>> Get()
        {
            var servicingInfo = this.servicingProvider.GetInfo();

            var loadingUnits = this.loadingUnitStatisticsProvider.GetWeightStatistics();

            var machine = this.machineProvider.Get();

            int? areaId = null;
            if (this.configuration.IsWmsEnabled())
            {
                var area = await this.machinesWmsWebService.GetAreaByIdAsync(machine.Id);
                areaId = area.Id;
            }

            var machineInfo = new MachineIdentity
            {
                AreaId = areaId,
                Id = machine.Id,
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
                TrayCount = loadingUnits.Count(),
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
                    var machine = this.machineProvider.Get();
                    var wmsMachine = await this.machinesWmsWebService.GetByIdAsync(machine.Id);

                    statistics.AreaFillPercentage = wmsMachine.AreaFillRate;
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
