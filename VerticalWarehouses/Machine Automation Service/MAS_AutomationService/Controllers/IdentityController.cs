using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly IGeneralInfoDataLayer generalInfo;

        #endregion

        #region Constructors

        public IdentityController(IGeneralInfoDataLayer generalInfo)
        {
            this.generalInfo = generalInfo;
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MachineIdentity))]
        [HttpGet]
        public async Task<ActionResult<MachineIdentity>> Get()
        {
            // TO DO implement genral info
            var machineInfo = new MachineIdentity
            {
                Id = 1,
                AreaId = 2,
                BayId = 3,
                Width = 3080,
                Depth = 500,
                ModelName = "VRT EF 84 L 990-BIS H 6345", //await this.generalInfo.GetModel(),
                SerialNumber = "VW_190012", // await this.generalInfo.GetSerial(),
                TrayCount = 42,
                MaxGrossWeight = 1000, // await this.generalInfo.GetMaxGrossWeight(),
                InstallationDate = System.DateTime.Now.AddMonths(-29),
                NextServiceDate = System.DateTime.Now.AddMonths(7),
                LastServiceDate = System.DateTime.Now.AddMonths(-9),
            };

            return this.Ok(machineInfo);
        }

        #endregion
    }
}
