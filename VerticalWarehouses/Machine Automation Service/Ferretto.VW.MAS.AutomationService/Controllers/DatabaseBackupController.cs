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

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public DatabaseBackupController(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IMachineProvider machineProvider,
            ILogger<DatabaseBackupController> logger)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
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
            //x return this.Ok(this.machineVolatileDataProvider.EnableLocalDbSavingOnServer);
            return this.Ok(this.machineProvider.IsDbSaveOnServer());
        }

        [HttpPost("get/backupOnTelemetry")]
        public ActionResult<bool> GetBackupOnTelemetry()
        {
            //x return this.Ok(this.machineVolatileDataProvider.EnableLocalDbSavingOnTelemetry);
            return this.Ok(this.machineProvider.IsDbSaveOnTelemetry());
        }

        [HttpPost("set/backupOnServer")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnServer(bool enable)
        {
            //x this.machineVolatileDataProvider.EnableLocalDbSavingOnServer = enable;
            this.machineProvider.UpdateDbSaveOnServer(enable);

            return this.Accepted();
        }

        [HttpPost("set/backupOnTelemetry")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnTelemetry(bool enable)
        {
            //x this.machineVolatileDataProvider.EnableLocalDbSavingOnTelemetry = enable;
            this.machineProvider.UpdateDbSaveOnTelemetry(enable);

            return this.Accepted();
        }

        #endregion
    }
}
