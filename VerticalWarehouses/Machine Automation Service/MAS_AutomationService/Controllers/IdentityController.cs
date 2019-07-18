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

        private readonly IGeneralInfo generalInfo;

        #endregion

        #region Constructors

        public IdentityController(IGeneralInfo generalInfo)
        {
            this.generalInfo = generalInfo;
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MachineIdentity))]
        [HttpGet]
        public async Task<ActionResult<MachineIdentity>> Get()
        {
            var machineInfo = new MachineIdentity
            {
                Id = 5,
                AreaId = 2,
                BayId = 3,
                Width = 3080,
                Depth = 500,
                ModelName = await this.generalInfo.Model,
                SerialNumber = await this.generalInfo.Serial,
                TrayCount = 42,
                MaxGrossWeight = await this.generalInfo.MaxGrossWeight,
                InstallationDate = await this.generalInfo.InstallationDate,
                NextServiceDate = System.DateTime.Now.AddMonths(7),
                LastServiceDate = System.DateTime.Now.AddMonths(-9),
            };

            return this.Ok(machineInfo);
        }

        #endregion
    }
}
