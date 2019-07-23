using Ferretto.VW.MAS.AutomationService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        internal static readonly MachineIdentity machineInfo = new MachineIdentity
        {
            Id = 5,
            AreaId = 2,
            BayId = 3,
            ModelName = "VRT EF 84 L 990-BIS H 6345",
            SerialNumber = "VW_190012",
            TrayCount = 42,
            InstallationDate = System.DateTime.Now.AddMonths(-29),
            NextServiceDate = System.DateTime.Now.AddMonths(7),
            LastServiceDate = System.DateTime.Now.AddMonths(-9),
            ServiceStatus = MachineServiceStatus.Valid
        };

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MachineIdentity))]
        [HttpGet]
        public ActionResult<MachineIdentity> Get()
        {
            return this.Ok(machineInfo);
        }

        #endregion
    }
}
