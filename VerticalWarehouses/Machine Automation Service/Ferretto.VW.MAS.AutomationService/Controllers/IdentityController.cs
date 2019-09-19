using System.Linq;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly IGeneralInfoConfigurationDataLayer generalInfo;

        private readonly ILoadingUnitsProvider loadingUnitStatisticsProvider;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            IGeneralInfoConfigurationDataLayer generalInfo,
            ILoadingUnitsProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IMachineConfigurationProvider machineConfigurationProvider)
        {
            if (generalInfo is null)
            {
                throw new System.ArgumentNullException(nameof(generalInfo));
            }

            if (loadingUnitStatisticsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            }

            if (servicingProvider is null)
            {
                throw new System.ArgumentNullException(nameof(servicingProvider));
            }

            if (machineConfigurationProvider == null)
            {
                throw new System.ArgumentNullException(nameof(machineConfigurationProvider));
            }

            this.generalInfo = generalInfo;
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider;
            this.servicingProvider = servicingProvider;
            this.machineConfigurationProvider = machineConfigurationProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<MachineIdentity> Get()
        {
            var servicingInfo = this.servicingProvider.GetInfo();

            var loadingUnits = this.loadingUnitStatisticsProvider.GetWeightStatistics();

            var machineInfo = new MachineIdentity
            {
                Id = 1,
                AreaId = 2, // TODO remove this hardcoded value
                Width = 3080, // TODO remove this hardcoded value
                Depth = 500, // TODO remove this hardcoded value
                ModelName = this.generalInfo.Model,
                SerialNumber = this.generalInfo.Serial,
                TrayCount = loadingUnits.Count(),
                MaxGrossWeight = this.generalInfo.MaxGrossWeight,
                InstallationDate = servicingInfo.InstallationDate,
                NextServiceDate = servicingInfo.NextServiceDate,
                LastServiceDate = servicingInfo.LastServiceDate,
                IsOneTonMachine = this.machineConfigurationProvider.IsOneKMachine(),
            };

            return this.Ok(machineInfo);
        }

        #endregion
    }
}
