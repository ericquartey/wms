using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsDataProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new System.ArgumentNullException(nameof(machineVolatileDataProvider));
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new System.ArgumentNullException(nameof(serviceScopeFactory));
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
                try
                {
                    var machinesWebService = this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMachinesWmsWebService>();
                    var area = await machinesWebService.GetAreaByIdAsync(machine.Id);
                    areaId = area.Id;
                }
                catch
                {
                }
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
                IsOneTonMachine = this.machineVolatileDataProvider.IsOneTonMachine.Value,
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
                    var machinesWebService = this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMachinesWmsWebService>();

                    var wmsMachine = await machinesWebService.GetByIdAsync(machine.Id);

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
