using System;
using System.Threading.Tasks;
using Ferretto.WMS.Scheduler.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        #region Methods

        [HttpPost("Abort")]
        public Task<ActionResult> Abort(Mission mission)
        {
            throw new NotImplementedException();
        }

        [HttpPost("Complete")]
        public Task<ActionResult> Complete(Mission mission)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
