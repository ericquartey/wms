using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitsDataProvider loadingUnitStatisticsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServicingProvider servicingProvider;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsDataProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IWmsSettingsProvider wmsSettingsProvider)
        {
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new System.ArgumentNullException(nameof(machineVolatileDataProvider));
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new System.ArgumentNullException(nameof(wmsSettingsProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<MachineIdentity>> Get([FromServices] IMachinesWmsWebService machinesWebService)
        {
            if (machinesWebService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesWebService));
            }

            var servicingInfo = this.servicingProvider.GetInstallationInfo();

            var loadingUnits = this.loadingUnitStatisticsProvider.GetWeightStatistics();

            var machine = this.machineProvider.Get();

            int? areaId = null;
            if (this.wmsSettingsProvider.IsEnabled)
            {
                try
                {
                    var area = await machinesWebService.GetAreaByIdAsync(machine.Id);
                    areaId = area.Id;
                }
                catch
                {
                    this.NotFound();
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
                InstallationDate = servicingInfo?.InstallationDate,
                NextServiceDate = servicingInfo?.NextServiceDate,
                LastServiceDate = servicingInfo?.LastServiceDate,
                IsOneTonMachine = this.machineVolatileDataProvider.IsOneTonMachine.Value,
                LoadingUnitDepth = machine.LoadUnitDepth,
                LoadingUnitWidth = machine.LoadUnitWidth,
            };

            return this.Ok(machineInfo);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<MachineStatistics>> GetStatistics([FromServices] IMachinesWmsWebService machinesWebService)
        {
            if (machinesWebService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesWebService));
            }

            var statistics = this.machineProvider.GetPresentStatistics();

            if (this.wmsSettingsProvider.IsEnabled)
            {
                try
                {
                    var machine = this.machineProvider.Get();

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
