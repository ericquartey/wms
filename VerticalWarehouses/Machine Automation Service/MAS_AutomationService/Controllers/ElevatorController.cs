using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

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
        {
            this.eventAggregator = eventAggregator;
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
            return 0;
        }

        [HttpPost("horizontal/move")]
        public void MoveHorizontal([FromBody]ElevatorMovementParameters data)
        {
            decimal maxSpeed = 0;
            decimal maxAcceleration = 0;
            decimal maxDeceleration = 0;
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;
            var movementType = MovementType.Relative;

            var homingDone = this.setupStatus.VerticalHomingDone;

            maxSpeed = this.horizontalAxis.MaxEmptySpeedHA;
            maxAcceleration = this.horizontalAxis.MaxEmptyAccelerationHA;
            maxDeceleration = this.horizontalAxis.MaxEmptyDecelerationHA;
            feedRate = this.horizontalManualMovements.FeedRateHM;

            if (homingDone)
            {
                initialTargetPosition = this.horizontalManualMovements.RecoveryTargetPositionHM;
            }
            else
            {
                initialTargetPosition = this.horizontalManualMovements.InitialTargetPositionHM;
            }

            // INFO +1 for Forward, -1 for Back
            initialTargetPosition *= data.Displacement;

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                movementType,
                MovementMode.Position,
                initialTargetPosition,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    messageData,
                    $"Execute {Axis.Horizontal} Positioning Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Positioning));

            this.logger.LogDebug($"Starting positioning on Axis {Axis.Horizontal}, type {data.MovementType}, target position {initialTargetPosition}");
        }

        [HttpPost("vertical/move")]
        public void MoveVertical([FromBody]ElevatorMovementParameters data)
        {
            decimal maxSpeed = 0;
            decimal maxAcceleration = 0;
            decimal maxDeceleration = 0;
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;
            var movementType = MovementType.Relative;

            var homingDone = this.setupStatus.VerticalHomingDone;

            maxSpeed = this.verticalAxis.MaxEmptySpeed;
            maxAcceleration = this.verticalAxis.MaxEmptyAcceleration;
            maxDeceleration = this.verticalAxis.MaxEmptyDeceleration;

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
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    messageData,
                    $"Execute {Axis.Horizontal} Positioning Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Positioning));

            this.logger.LogDebug($"Starting positioning on Axis {Axis.Horizontal}, type {data.MovementType}, target position {initialTargetPosition}");
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                   new CommandMessage(
                       null,
                       "Stop Command",
                       MessageActor.FiniteStateMachines,
                       MessageActor.WebApi,
                       MessageType.Stop));

            return this.Ok();
        }

        #endregion
    }
}
