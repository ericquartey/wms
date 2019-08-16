using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : BaseAutomationController
    {
        #region Fields

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILogger logger;

        private readonly ISetupStatusDataLayer setupStatus;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovements;

        #endregion

        #region Constructors

        public ElevatorController(
            IEventAggregator eventAggregator,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            ISetupStatusDataLayer setupStatusDataLayer,
            ILogger<ElevatorController> logger)
            : base(eventAggregator)
        {
            if (verticalAxisDataLayer == null)
            {
                throw new System.ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (verticalManualMovementsDataLayer == null)
            {
                throw new System.ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            if (horizontalAxisDataLayer == null)
            {
                throw new System.ArgumentNullException(nameof(horizontalAxisDataLayer));
            }

            if (horizontalManualMovementsDataLayer == null)
            {
                throw new System.ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (setupStatusDataLayer == null)
            {
                throw new System.ArgumentNullException(nameof(setupStatusDataLayer));
            }

            if (logger == null)
            {
                throw new System.ArgumentNullException(nameof(logger));
            }

            this.verticalAxis = verticalAxisDataLayer;
            this.verticalManualMovements = verticalManualMovementsDataLayer;
            this.horizontalAxis = horizontalAxisDataLayer;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
            this.setupStatus = setupStatusDataLayer;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpGet("vertical/position")]
        public ActionResult<decimal> GetVerticalPosition()
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("horizontal/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontal([FromBody]ElevatorMovementParameters data)
        {
            var initialTargetPosition = this.setupStatus.VerticalHomingDone
                ? this.horizontalManualMovements.RecoveryTargetPositionHM
                : this.horizontalManualMovements.InitialTargetPositionHM;

            // INFO +1 for Forward, -1 for Back
            // TODO: this is not very clear, rethink about it
            initialTargetPosition *= data.Displacement;

            var speed = this.horizontalAxis.MaxEmptySpeedHA * this.horizontalManualMovements.FeedRateHM;

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.Position,
                initialTargetPosition,
                speed,
                this.horizontalAxis.MaxEmptyAccelerationHA,
                this.horizontalAxis.MaxEmptyDecelerationHA,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            this.logger.LogDebug($"Starting positioning on Axis {Axis.Horizontal}, type {MovementType.Relative}, target position {initialTargetPosition}");

            return this.Accepted();
        }

        [HttpPost("vertical/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVertical([FromBody]ElevatorMovementParameters data)
        {
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;
            var movementType = MovementType.Relative;

            var homingDone = this.setupStatus.VerticalHomingDone;

            var maxSpeed = this.verticalAxis.MaxEmptySpeed;
            var maxAcceleration = this.verticalAxis.MaxEmptyAcceleration;
            var maxDeceleration = this.verticalAxis.MaxEmptyDeceleration;

            //INFO Absolute movement using the min and max reachable positions for limits
            if (homingDone)
            {
                feedRate = this.verticalManualMovements.FeedRateAfterZero;
                movementType = MovementType.Absolute;
                //INFO For movements Up the limit is the UpperBound, for movements down the limit is the LowerBound
                initialTargetPosition = data.Displacement > 0 ? this.verticalAxis.UpperBound : this.verticalAxis.LowerBound;
            }
            else //INFO Before homing relative movements step by step
            {
                feedRate = this.verticalManualMovements.FeedRateVM;
                //INFO +1 for Up, -1 for Down
                initialTargetPosition = data.Displacement > 0 ? this.verticalManualMovements.PositiveTargetDirection : -this.verticalManualMovements.NegativeTargetDirection;
            }

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                movementType,
                MovementMode.Position,
                initialTargetPosition,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            this.logger.LogDebug($"Starting positioning on Axis {Axis.Horizontal}, type {data.MovementType}, target position {initialTargetPosition}");

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
