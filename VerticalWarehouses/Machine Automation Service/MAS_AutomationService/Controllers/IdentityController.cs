using System.Linq;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
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

        private readonly ILoadingUnitStatisticsDataLayer loadingUnitStatistics;

        private readonly IServicingProvider servicingProvider;

        #endregion

        #region Constructors

        public IdentityController(
            IGeneralInfoConfigurationDataLayer generalInfo,
            ILoadingUnitStatisticsDataLayer loadingUnitStatistics,
            IServicingProvider servicingProvider)
        {
            if (generalInfo == null)
            {
                throw new System.ArgumentNullException(nameof(generalInfo));
            }

            if (loadingUnitStatistics == null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatistics));
            }

            if (servicingProvider == null)
            {
                throw new System.ArgumentNullException(nameof(servicingProvider));
            }

            this.generalInfo = generalInfo;
            this.loadingUnitStatistics = loadingUnitStatistics;
            this.servicingProvider = servicingProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MachineIdentity))]
        [HttpGet]
        public ActionResult<MachineIdentity> Get()
        {
            var servicingInfo = this.servicingProvider.GetInfo();

            var loadingUnits = this.loadingUnitStatistics.GetWeightStatistics();

            // TO DO implement genral info
            var machineInfo = new MachineIdentity
            {
                Id = 1,
                AreaId = 2, // TODO
                BayId = 3, // TODO
                Width = 3080, // TODO
                Depth = 500, // TODO
                ModelName = "VRT EF 84 L 990-BIS H 6345", // TODO await this.generalInfo.GetModel(),
                SerialNumber = "VW_190012", // TODO await this.generalInfo.GetSerial(),
                TrayCount = loadingUnits.Count(),
                MaxGrossWeight = 1000, // TODO await this.generalInfo.GetMaxGrossWeight(),
                InstallationDate = servicingInfo.InstallationDate,
                NextServiceDate = servicingInfo.NextServiceDate,
                LastServiceDate = servicingInfo.LastServiceDate,
            };

            return this.Ok(machineInfo);
        }

        #endregion
    }
}
