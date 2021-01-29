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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public DatabaseBackupController(IMachineVolatileDataProvider machineVolatileDataProvider,
            ILogger<DatabaseBackupController> logger)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
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
            return this.Ok(this.machineVolatileDataProvider.EnableLocalDbSavingOnServer);
        }

        [HttpPost("get/backupOnTelemetry")]
        public ActionResult<bool> GetBackupOnTelemetry()
        {
            return this.Ok(this.machineVolatileDataProvider.EnableLocalDbSavingOnTelemetry);
        }

        [HttpPost("set/backupOnServer")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnServer(bool enable)
        {
            this.machineVolatileDataProvider.EnableLocalDbSavingOnServer = enable;

            return this.Accepted();
        }

        [HttpPost("set/backupOnTelemetry")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnTelemetry(bool enable)
        {
            this.machineVolatileDataProvider.EnableLocalDbSavingOnTelemetry = enable;

            return this.Accepted();
        }

        #endregion
    }
}
