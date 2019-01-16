using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController : ControllerBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion Fields

        #region Constructors

        public BaysController(ILogger<BaysController> logger)
        {
            this.logger = logger;
        }

        #endregion Constructors

        #region Methods

        [HttpPost("MarkAsOperational")]
        public async Task<ActionResult> MarkAsOperational(int bayId)
        {
            this.logger.LogInformation($"Bay {bayId} is now operational.");
            await Task.Delay(100);
            return this.Ok();
        }

        #endregion Methods
    }
}
