using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarouselController : BaseAutomationController
    {
        #region Fields

        private readonly IBayChainProvider bayChainProvider;

        #endregion

        #region Constructors

        public CarouselController(
            IEventAggregator eventAggregator,
            IBayChainProvider bayChainProvider)
            : base(eventAggregator)
        {
            this.bayChainProvider = bayChainProvider ?? throw new ArgumentNullException(nameof(bayChainProvider));
        }

        #endregion

        #region Methods

        [HttpPost("findzero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, Calibration.FindSensor);

            this.PublishCommand(
                homingData,
                "Execute FindZeroSensor Command",
                MessageActor.FiniteStateMachines,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpGet("position")]
        public ActionResult<double> GetPosition()
        {
            return this.Ok(this.bayChainProvider.HorizontalPosition);
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, Calibration.ResetEncoder);

            this.PublishCommand(
                homingData,
                "Execute Homing Command",
                MessageActor.FiniteStateMachines,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult Move(HorizontalMovementDirection direction)
        {
            this.bayChainProvider.Move(direction, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManual(HorizontalMovementDirection direction)
        {
            this.bayChainProvider.MoveManual(direction, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.bayChainProvider.Stop(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
