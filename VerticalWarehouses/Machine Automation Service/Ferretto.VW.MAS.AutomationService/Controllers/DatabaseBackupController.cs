using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseBackupController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly ILogger<DatabaseBackupController> logger;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public DatabaseBackupController(
            IMachineProvider machineProvider,
            ILogger<DatabaseBackupController> logger)
        {
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet("get/backupOnServer")]
        public ActionResult<bool> GetBackupOnServer()
        {
            return this.Ok(this.machineProvider.IsDbSaveOnServer());
        }

        [HttpPost("get/backupOnTelemetry")]
        public ActionResult<bool> GetBackupOnTelemetry()
        {
            return this.Ok(this.machineProvider.IsDbSaveOnTelemetry());
        }

        [HttpPost("set/backupOnServer")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnServer(bool enable)
        {
            this.machineProvider.UpdateDbSaveOnServer(enable);

            return this.Accepted();
        }

        [HttpPost("set/backupOnTelemetry")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnTelemetry(bool enable)
        {
            this.machineProvider.UpdateDbSaveOnTelemetry(enable);

            return this.Accepted();
        }

        #endregion
    }
}
