using System.Linq;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitsProvider loadingUnitStatisticsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineProvider machineProvider)
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

            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider;
            this.servicingProvider = servicingProvider;
            this.machineProvider = machineProvider;
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
                MaxGrossWeight = machine.MaxGrossWeight,
                InstallationDate = servicingInfo.InstallationDate,
                NextServiceDate = servicingInfo.NextServiceDate,
                LastServiceDate = servicingInfo.LastServiceDate,
                IsOneTonMachine = this.machineProvider.IsOneTonMachine(),
            };

            return this.Ok(machineInfo);
        }

        [HttpGet("machine")]
        public ActionResult<Machine> GetMachine()
        {
            var machine = this.machineProvider.Get();

            return this.Ok(machine);
        }

        #endregion
    }
}
