using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
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

        private readonly IDataLayerService dataLayerService;

        private readonly ILogger<DatabaseBackupController> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatile;

        #endregion

        #region Constructors

        public DatabaseBackupController(
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatile,
            IDataLayerService dataLayerService,
            ILogger<DatabaseBackupController> logger)
        {
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineVolatile = machineVolatile ?? throw new ArgumentNullException(nameof(machineVolatile));
            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));
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

        [HttpGet("get/backupServer")]
        public ActionResult<string> GetBackupServer()
        {
            return this.Ok(this.machineProvider.GetBackupServer());
        }

        [HttpGet("get/backupServerPassword")]
        public ActionResult<string> GetBackupServerPassword()
        {
            return this.Ok(this.machineProvider.GetBackupServerPassword());
        }

        [HttpGet("get/backupServerUsername")]
        public ActionResult<string> GetBackupServerUsername()
        {
            return this.Ok(this.machineProvider.GetBackupServerUsername());
        }

        [HttpGet("get/standbydb")]
        public ActionResult<bool> GetStandbyDb()
        {
            return this.Ok(this.machineVolatile.IsStandbyDbOk);
        }

        [HttpPost("set/backupOnServer")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBackupOnServer(bool enable, string server, string username, string password)
        {
            this.machineProvider.UpdateDbSaveOnServer(enable, server, username, password);

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

        [HttpPost("test/backupOnServer")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult TestBackupOnServer(string server, string username, string password)
        {
            this.dataLayerService.CopyMachineDatabaseToServer(server, username, password, this.machineProvider.GetSecondaryDatabase(), this.machineProvider.GetSerialNumber());

            return this.Accepted();
        }

        #endregion
    }
}
