using System;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : BaseAutomationController
    {
        #region Fields

        private readonly IBayPositionControlDataLayer bayPositionControl;

        private readonly ICellControlDataLayer cellControlDataLayer;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly ILogger logger;

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovements;

        private readonly IWeightControlDataLayer weightControl;

        #endregion

        #region Constructors

        public ElevatorController(
            IEventAggregator eventAggregator,
            IWeightControlDataLayer weightControl,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            ISetupStatusProvider setupStatusProvider,
            IBayPositionControlDataLayer bayPositionControl,
            IElevatorProvider elevatorProvider,
            ICellControlDataLayer cellControlDataLayer,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<ElevatorController> logger)
            : base(eventAggregator)
        {
            if (weightControl is null)
            {
                throw new System.ArgumentNullException(nameof(weightControl));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (verticalManualMovementsDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            if (horizontalAxisDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(horizontalAxisDataLayer));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (offsetCalibrationDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(offsetCalibrationDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new System.ArgumentNullException(nameof(setupStatusProvider));
            }

            if (bayPositionControl is null)
            {
                throw new System.ArgumentNullException(nameof(bayPositionControl));
            }

            if (elevatorProvider is null)
            {
                throw new System.ArgumentNullException(nameof(elevatorProvider));
            }

            if (cellControlDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(cellControlDataLayer));
            }

            if (logger is null)
            {
                throw new System.ArgumentNullException(nameof(logger));
            }

            this.weightControl = weightControl;
            this.verticalAxis = verticalAxisDataLayer;
            this.verticalManualMovements = verticalManualMovementsDataLayer;
            this.horizontalAxis = horizontalAxisDataLayer;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
            this.offsetCalibrationDataLayer = offsetCalibrationDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.bayPositionControl = bayPositionControl;
            this.elevatorProvider = elevatorProvider;
            this.cellControlDataLayer = cellControlDataLayer;
            this.installationHub = installationHub;
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

            return this.Ok(notifyData.CurrentPosition);
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

            return this.Ok(notifyData.CurrentPosition);
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory)
        {
            var lowerBound = this.verticalAxis.LowerBound;
            var upperBound = this.verticalAxis.UpperBound;

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = string.Format(Resources.Elevator.TargetPositionMustBeInRange, targetPosition, lowerBound, upperBound)
                    });
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                return this.UnprocessableEntity(
                    new ProblemDetails
                    {
                        Title = Resources.General.UnprocessableEntityTitle,
                        Detail = Resources.Elevator.VerticalOriginCalibrationMustBePerformed
                    });
            }

            var feedRate = this.GetFeedRate(feedRateCategory);

            var speed = this.verticalAxis.MaxEmptySpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
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

            this.logger.LogDebug($"Starting elevator absolute vertical positioning, target position is '{targetPosition}'.");

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
            if (distance == 0)
            {
                return this.BadRequest(
                  new ProblemDetails
                  {
                      Title = Resources.General.BadRequestTitle,
                      Detail = Resources.Elevator.MovementDistanceCannotBeZero
                  });
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                return this.UnprocessableEntity(
                   new ProblemDetails
                   {
                       Title = Resources.General.UnprocessableEntityTitle,
                       Detail = Resources.Elevator.VerticalOriginCalibrationMustBePerformed
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

        [HttpPost("weight-check-stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopWeightCheck()
        {
            // TO DO
            return this.Accepted();
        }

        [HttpPost("{id}/Weight-Check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult WeightCheck(int loadingUnitId, decimal runToTest, decimal weight)
        {
            try
            {
                this.elevatorProvider.Start(loadingUnitId, runToTest, weight);
                var data = new ElevatorWeightCheckMessageData() { Weight = 200 };
                this.installationHub.Clients.All.ElevatorWeightCheck(new NotificationMessageUI<IElevatorWeightCheckMessageData>() { Data = data });
                return this.Ok();
            }
            catch (DataLayer.Exceptions.EntityNotFoundException ex)
            {
                return this.NotFound(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Detail = ex.Message
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return this.BadRequest(new Microsoft.AspNetCore.Mvc.ProblemDetails { Detail = ex.Message });
            }
        }

        private decimal GetFeedRate(FeedRateCategory feedRateCategory)
        {
            decimal feedRate = 0;
            switch (feedRateCategory)
            {
                case FeedRateCategory.Undefined:
                    throw new System.NotImplementedException(nameof(FeedRateCategory.Undefined));

                case FeedRateCategory.VerticalManualMovements:
                    feedRate = this.verticalManualMovements.FeedRateVM;
                    break;

                case FeedRateCategory.VerticalManualMovementsAfterZero:
                    feedRate = this.verticalManualMovements.FeedRateAfterZero;
                    break;

                case FeedRateCategory.HorizontalManualMovements:
                    feedRate = this.horizontalManualMovements.FeedRateHM;
                    break;

                case FeedRateCategory.ShutterManualMovements:
                    throw new System.NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                case FeedRateCategory.ResolutionCalibration:
                    throw new System.NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                case FeedRateCategory.OffsetCalibration:
                    feedRate = this.offsetCalibrationDataLayer.FeedRateOC;
                    break;

                case FeedRateCategory.CellControl:
                    feedRate = this.cellControlDataLayer.FeedRateCC;
                    break;

                case FeedRateCategory.PanelControl:
                    throw new System.NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                case FeedRateCategory.ShutterHeightControl:
                    throw new System.NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                case FeedRateCategory.LoadingUnitWeight:
                    feedRate = this.weightControl.FeedRateWC;
                    break;

                case FeedRateCategory.BayHeight:
                    feedRate = this.bayPositionControl.FeedRateBP;
                    break;

                case FeedRateCategory.LoadFirstDrawer:
                    throw new System.NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                default:
                    break;
            }

            return feedRate;
        }

        #endregion
    }
}
