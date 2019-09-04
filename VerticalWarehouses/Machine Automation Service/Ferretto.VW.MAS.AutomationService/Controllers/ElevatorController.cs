using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
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

        private readonly ISetupStatusProvider setupStatusProvider;

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
            ISetupStatusProvider setupStatusProvider,
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

            if (setupStatusProvider == null)
            {
                throw new System.ArgumentNullException(nameof(setupStatusProvider));
            }

            if (logger == null)
            {
                throw new System.ArgumentNullException(nameof(logger));
            }

            this.verticalAxis = verticalAxisDataLayer;
            this.verticalManualMovements = verticalManualMovementsDataLayer;
            this.horizontalAxis = horizontalAxisDataLayer;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.logger = logger;
        }

        #endregion



        #region Methods

        [HttpGet("horizontal/position")]
        public ActionResult<decimal> GetHorizontalPosition()
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, 0);

            void publishAction() => this.PublishCommand(
                messageData,
                "Request Horizontal position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);

            this.logger.LogDebug($"Request position on Axis {Axis.Horizontal}");

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            if (notifyData?.CurrentPosition == null)
            {
                throw new System.Exception("Cannot get current vertical position.");
            }

            return this.Ok(notifyData?.CurrentPosition);
        }

        [HttpGet("vertical/position")]
        public ActionResult<decimal> GetVerticalPosition()
        {
            var messageData = new RequestPositionMessageData(Axis.Vertical, 0);
            void publishAction() => this.PublishCommand(
                messageData,
                "Request vertical position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);

            this.logger.LogDebug($"Request position on Axis {Axis.Vertical}");

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            if (notifyData?.CurrentPosition == null)
            {
                throw new System.Exception("Cannot get current vertical position.");
            }

            return this.Ok(notifyData?.CurrentPosition);
        }

        [HttpPost("horizontal/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontal(HorizontalMovementDirection direction)
        {
            var setupStatus = this.setupStatusProvider.Get();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? this.horizontalManualMovements.RecoveryTargetPositionHM
                : this.horizontalManualMovements.InitialTargetPositionHM;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = this.horizontalAxis.MaxEmptySpeedHA * this.horizontalManualMovements.FeedRateHM;

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.Position,
                targetPosition,
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

            this.logger.LogDebug($"Starting positioning on Axis {Axis.Horizontal}, type {MovementType.Relative}, target position {targetPosition}");

            return this.Accepted();
        }

        [HttpPost("vertical/move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(decimal targetPosition)
        {
            var lowerBound = this.verticalAxis.LowerBound;
            var upperBound = this.verticalAxis.UpperBound;

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Detail = $"Target position ({targetPosition}) must be in the range [{lowerBound}; {upperBound}]."
                    });
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                return this.UnprocessableEntity(
                    new ProblemDetails
                    {
                        Detail = $"Vertical origin calibration must be performed before attempting to move the elevator to a given position."
                    });
            }

            var movementType = MovementType.Absolute;

            var feedRate = this.verticalManualMovements.FeedRateAfterZero;

            var speed = this.verticalAxis.MaxEmptySpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                movementType,
                MovementMode.Position,
                targetPosition,
                speed,
                this.verticalAxis.MaxEmptyAcceleration, // TODO is this correct?
                this.verticalAxis.MaxEmptyDeceleration, // TODO is this correct?
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            this.logger.LogDebug($"Starting elevator positioning, type {movementType}, target position {targetPosition}");

            return this.Accepted();
        }

        [HttpPost("vertical/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVertical(VerticalMovementDirection direction)
        {
            var movementType = MovementType.Relative;

            decimal feedRate;
            decimal targetPosition;

            //INFO Absolute movement using the min and max reachable positions for limits
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (homingDone)
            {
                feedRate = this.verticalManualMovements.FeedRateAfterZero;
                movementType = MovementType.Absolute;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? this.verticalAxis.UpperBound
                    : this.verticalAxis.LowerBound;
            }
            else //INFO Before homing relative movements step by step
            {
                feedRate = this.verticalManualMovements.FeedRateVM;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? this.verticalManualMovements.PositiveTargetDirection
                    : -this.verticalManualMovements.NegativeTargetDirection;
            }

            var speed = this.verticalAxis.MaxEmptySpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                movementType,
                MovementMode.Position,
                targetPosition,
                speed,
                this.verticalAxis.MaxEmptyAcceleration,
                this.verticalAxis.MaxEmptyDeceleration,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            this.logger.LogDebug($"Starting positioning on Axis {Axis.Horizontal}, type {movementType}, target position {targetPosition}");

            return this.Accepted();
        }

        [HttpPost("vertical/move-relative")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalOfDistance(decimal distance)
        {
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                return this.UnprocessableEntity(
                   new ProblemDetails
                   {
                       Detail = $"Vertical origin calibration must be performed before attempting to move the elevator of a given relative position."
                   });
            }

            var speed = this.verticalAxis.MaxEmptySpeed * this.verticalManualMovements.FeedRateAfterZero;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                distance,
                speed,
                this.verticalAxis.MaxEmptyAcceleration,
                this.verticalAxis.MaxEmptyDeceleration,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            this.logger.LogDebug($"Starting vertical axis movement, displacement={distance}");

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
