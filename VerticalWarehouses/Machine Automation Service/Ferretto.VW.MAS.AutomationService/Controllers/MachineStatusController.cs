using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.MAS.MissionsManager.Providers;
using Microsoft.AspNetCore.Http;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineStatusController : BaseAutomationController
    {
        #region Fields

        private readonly IRunningStateProvider runningStateProvider;

        #endregion

        #region Constructors

        public MachineStatusController(
            IRunningStateProvider runningStateProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.runningStateProvider = runningStateProvider ?? throw new ArgumentNullException(nameof(runningStateProvider));
        }

        #endregion

        #region Methods

        [HttpGet("is-powered-on")]
        public ActionResult<bool> IsPoweredOn()
        {
            return this.runningStateProvider.IsRunning;
        }

        [HttpPost("power-off")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PowerOff()
        {
            try
            {
                this.runningStateProvider.SetRunningState(false, this.BayNumber, MessageActor.AutomationService);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("power-on")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PowerOn()
        {
            try
            {
                this.runningStateProvider.SetRunningState(true, this.BayNumber, MessageActor.AutomationService);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Stop()
        {
            try
            {
                this.runningStateProvider.Stop(this.BayNumber, MessageActor.AutomationService);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        #endregion
    }
}
