using System;
using System.Collections.Generic;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.TelemetryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        #region Fields

        private readonly IErrorLogProvider errorLogProvider;

        private readonly IIOLogProvider ioLogProvider;

        private readonly IMissionLogProvider missionLogProvider;

        private readonly IScreenShotProvider screenShotProvider;

        #endregion

        #region Constructors

        public LogsController(
            IErrorLogProvider errorLogProvider,
            IIOLogProvider ioLogProvider,
            IMissionLogProvider missionLogProvider,
            IScreenShotProvider screenShotProvider)
        {
            this.errorLogProvider = errorLogProvider ?? throw new ArgumentNullException(nameof(errorLogProvider));
            this.ioLogProvider = ioLogProvider ?? throw new ArgumentNullException(nameof(ioLogProvider));
            this.missionLogProvider = missionLogProvider ?? throw new ArgumentNullException(nameof(missionLogProvider));
            this.screenShotProvider = screenShotProvider ?? throw new ArgumentNullException(nameof(screenShotProvider));
        }

        #endregion

        #region Methods

        [HttpGet("errors")]
        public ActionResult<IEnumerable<IErrorLog>> GetErrorLogs()
        {
            return this.Ok(this.errorLogProvider.GetAll());
        }

        [HttpGet("io")]
        public ActionResult<IEnumerable<IIOLog>> GetIOLogs()
        {
            return this.Ok(this.ioLogProvider.GetAll());
        }

        [HttpGet("missions")]
        public ActionResult<IEnumerable<IMissionLog>> GetMissionLogs()
        {
            return this.Ok(this.missionLogProvider.GetAll());
        }

        [HttpGet("screenshots")]
        public ActionResult<IEnumerable<IScreenShot>> GetScreenShots()
        {
            return this.Ok(this.screenShotProvider.GetAll());
        }

        #endregion
    }
}
