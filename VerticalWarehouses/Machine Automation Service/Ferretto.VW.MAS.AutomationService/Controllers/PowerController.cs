using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PowerController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IRunningStateProvider runningStateProvider;

        #endregion

        #region Constructors

        public PowerController(
            IRunningStateProvider runningStateProvider)
        {
            this.runningStateProvider = runningStateProvider ?? throw new ArgumentNullException(nameof(runningStateProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<CommonUtils.Messages.MachinePowerState> Get()
        {
            return this.runningStateProvider.MachinePowerState;
        }

        [HttpGet("power-ishoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Dictionary<BayNumber, bool>> GetIsHoming()
        {
            return this.Ok(this.runningStateProvider?.IsBayHoming);
        }

        [HttpPost("power-off")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PowerOff()
        {
            this.runningStateProvider.SetRunningState(false, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("power-on")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PowerOn()
        {
            this.runningStateProvider.SetRunningState(true, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Stop()
        {
            this.runningStateProvider.Stop(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
