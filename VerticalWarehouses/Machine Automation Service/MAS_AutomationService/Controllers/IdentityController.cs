using System.Linq;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly IGeneralInfoConfigurationDataLayer generalInfo;

        private readonly ILoadingUnitStatisticsProvider loadingUnitStatisticsProvider;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            IGeneralInfoConfigurationDataLayer generalInfo,
            ILoadingUnitStatisticsProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider)
        {
            if (generalInfo == null)
            {
                throw new System.ArgumentNullException(nameof(generalInfo));
            }

            if (loadingUnitStatisticsProvider == null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            }

            if (servicingProvider == null)
            {
                throw new System.ArgumentNullException(nameof(servicingProvider));
            }

            this.generalInfo = generalInfo;
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider;
            this.servicingProvider = servicingProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MachineIdentity))]
        [HttpGet]
        public ActionResult<MachineIdentity> Get()
        {
            var servicingInfo = this.servicingProvider.GetInfo();

            var loadingUnits = this.loadingUnitStatisticsProvider.GetWeightStatistics();

            // TO DO implement general info
            var machineInfo = new MachineIdentity
            {
                Id = 1,
                AreaId = 2, // TODO
                BayId = 3, // TODO
                Width = 3080, // TODO
                Depth = 500, // TODO
                ModelName = this.generalInfo.Model,
                SerialNumber = this.generalInfo.Serial,
                TrayCount = loadingUnits.Count(),
                MaxGrossWeight = this.generalInfo.MaxGrossWeight,
                InstallationDate = servicingInfo.InstallationDate,
                NextServiceDate = servicingInfo.NextServiceDate,
                LastServiceDate = servicingInfo.LastServiceDate,
            };

            return this.Ok(machineInfo);
        }

        #endregion
    }
}
