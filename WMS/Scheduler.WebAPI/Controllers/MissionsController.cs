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
        #region Fields

        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        #endregion Fields

        #region Constructors

        public MissionsController(
            IServiceProvider serviceProvider,
            ILogger<ItemsController> logger)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        #endregion Constructors

        #region Methods

        [HttpPost("Abort")]
        public async Task<ActionResult> Abort(Mission mission)
        {
            await Task.Delay(1000);
            this.logger.LogInformation($"Mission {mission} aborted.");
            return this.Ok();
        }

        [HttpPost("Complete")]
        public async Task<ActionResult> Complete(Mission mission)
        {
            await Task.Delay(1000);
            this.logger.LogInformation($"Mission {mission} completed.");
            return this.Ok();
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(int bayId)
        {
            await Task.Delay(100);
            return this.Ok();
        }

        #endregion Methods
    }
}
