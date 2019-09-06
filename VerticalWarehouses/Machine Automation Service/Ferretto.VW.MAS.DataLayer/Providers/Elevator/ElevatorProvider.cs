using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class ElevatorProvider : BaseProvider, IElevatorProvider
    {
        #region Fields

        private readonly IBayPositionControlDataLayer bayPositionControl;

        private readonly ICellControlDataLayer cellControlDataLayer;

        private readonly DataLayerContext dataContext;

        private readonly IHorizontalAxisDataLayer horizontalAxisDataLayer;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer;

        private readonly IHorizontalMovementBackwardProfileDataLayer horizontalMovementBackwardProfileDataLayer;

        private readonly IHorizontalMovementForwardProfileDataLayer horizontalMovementForwardProfileDataLayer;

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly IPanelControlDataLayer panelControlDataLayer;

        private readonly IResolutionCalibrationDataLayer resolutionCalibrationDataLayer;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxisDataLayer;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer;

        private readonly IWeightControlDataLayer weightControl;

        #endregion

        #region Constructors

        public ElevatorProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IPanelControlDataLayer panelControlDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            IHorizontalMovementBackwardProfileDataLayer horizontalMovementBackwardProfileDataLayer,
            IHorizontalMovementForwardProfileDataLayer horizontalMovementForwardProfileDataLayer,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IResolutionCalibrationDataLayer resolutionCalibrationDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IBayPositionControlDataLayer bayPositionControl,
            ICellControlDataLayer cellControlDataLayer,
            ISetupStatusProvider setupStatusProvider,
            IWeightControlDataLayer weightControl,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer)
            : base(eventAggregator)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (panelControlDataLayer is null)
            {
                throw new ArgumentNullException(nameof(panelControlDataLayer));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            if (horizontalMovementBackwardProfileDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementBackwardProfileDataLayer));
            }

            if (horizontalMovementForwardProfileDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementForwardProfileDataLayer));
            }

            if (horizontalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalAxisDataLayer));
            }

            if (resolutionCalibrationDataLayer is null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibrationDataLayer));
            }

            if (offsetCalibrationDataLayer is null)
            {
                throw new ArgumentNullException(nameof(offsetCalibrationDataLayer));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (bayPositionControl is null)
            {
                throw new ArgumentNullException(nameof(bayPositionControl));
            }

            if (cellControlDataLayer is null)
            {
                throw new ArgumentNullException(nameof(cellControlDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            if (weightControl is null)
            {
                throw new ArgumentNullException(nameof(weightControl));
            }

            if (verticalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            this.dataContext = dataContext;
            this.panelControlDataLayer = panelControlDataLayer;
            this.horizontalManualMovementsDataLayer = horizontalManualMovementsDataLayer;
            this.horizontalMovementBackwardProfileDataLayer = horizontalMovementBackwardProfileDataLayer;
            this.horizontalMovementForwardProfileDataLayer = horizontalMovementForwardProfileDataLayer;
            this.horizontalAxisDataLayer = horizontalAxisDataLayer;
            this.resolutionCalibrationDataLayer = resolutionCalibrationDataLayer;
            this.offsetCalibrationDataLayer = offsetCalibrationDataLayer;
            this.verticalAxisDataLayer = verticalAxisDataLayer;
            this.bayPositionControl = bayPositionControl;
            this.cellControlDataLayer = cellControlDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.weightControl = weightControl;
            this.verticalManualMovementsDataLayer = verticalManualMovementsDataLayer;
        }

        #endregion

        #region Methods

        public decimal GetHorizontalPosition()
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, 0);

            void publishAction() => this.PublishCommand(
                messageData,
                "Request Horizontal position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition;
        }

        public decimal GetVerticalPosition()
        {
            var messageData = new RequestPositionMessageData(Axis.Vertical, 0);

            void publishAction() => this.PublishCommand(
                messageData,
                "Request vertical position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition;
        }

        public void MoveHorizontal(HorizontalMovementDirection direction)
        {
            var setupStatus = this.setupStatusProvider.Get();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? this.horizontalManualMovementsDataLayer.RecoveryTargetPositionHM
                : this.horizontalManualMovementsDataLayer.InitialTargetPositionHM;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            decimal[] speed = { this.horizontalAxisDataLayer.MaxEmptySpeedHA * this.horizontalManualMovementsDataLayer.FeedRateHM / 10 };
            decimal[] acceleration = { this.horizontalAxisDataLayer.MaxEmptyAccelerationHA };
            decimal[] deceleration = { this.horizontalAxisDataLayer.MaxEmptyDecelerationHA };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void MoveHorizontalTableTravel(HorizontalMovementDirection direction)
        {
            var setupStatus = this.setupStatusProvider.Get();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? this.horizontalManualMovementsDataLayer.RecoveryTargetPositionHM
                : this.horizontalManualMovementsDataLayer.InitialTargetPositionHM;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            decimal[] speed = {
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P0SpeedV1 : this.horizontalMovementBackwardProfileDataLayer.P0SpeedV1,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P1SpeedV2 : this.horizontalMovementBackwardProfileDataLayer.P1SpeedV2,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P2SpeedV3 : this.horizontalMovementBackwardProfileDataLayer.P2SpeedV3,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P3SpeedV4 : this.horizontalMovementBackwardProfileDataLayer.P3SpeedV4,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P4SpeedV5 : this.horizontalMovementBackwardProfileDataLayer.P4SpeedV5
            };
            decimal[] acceleration = {
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P0Acceleration : this.horizontalMovementBackwardProfileDataLayer.P0Acceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P1Acceleration : this.horizontalMovementBackwardProfileDataLayer.P1Acceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P2Acceleration : this.horizontalMovementBackwardProfileDataLayer.P2Acceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P3Acceleration : this.horizontalMovementBackwardProfileDataLayer.P3Acceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P4Acceleration : this.horizontalMovementBackwardProfileDataLayer.P4Acceleration
            };
            decimal[] deceleration = {
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P0Deceleration : this.horizontalMovementBackwardProfileDataLayer.P0Deceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P1Deceleration : this.horizontalMovementBackwardProfileDataLayer.P1Deceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P2Deceleration : this.horizontalMovementBackwardProfileDataLayer.P2Deceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P3Deceleration : this.horizontalMovementBackwardProfileDataLayer.P3Deceleration,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P4Deceleration : this.horizontalMovementBackwardProfileDataLayer.P4Deceleration
            };
            decimal[] switchPosition = {
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P0Quote : this.horizontalMovementBackwardProfileDataLayer.P0Quote,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P1Quote : this.horizontalMovementBackwardProfileDataLayer.P1Quote,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P2Quote : this.horizontalMovementBackwardProfileDataLayer.P2Quote,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P3Quote : this.horizontalMovementBackwardProfileDataLayer.P3Quote,
                direction == HorizontalMovementDirection.Forwards ? this.horizontalMovementForwardProfileDataLayer.P4Quote : this.horizontalMovementBackwardProfileDataLayer.P4Quote
            };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory)
        {
            var lowerBound = Math.Max(this.verticalAxisDataLayer.LowerBound, this.verticalAxisDataLayer.Offset);
            var upperBound = this.verticalAxisDataLayer.UpperBound;

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetPosition),
                    string.Format(Resources.Elevator.TargetPositionMustBeInRange, targetPosition, lowerBound, upperBound));
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                throw new InvalidOperationException(
                   Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            var feedRate = this.GetFeedRate(feedRateCategory);

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * feedRate };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void MoveVertical(VerticalMovementDirection direction)
        {
            var movementType = MovementType.Relative;

            decimal feedRate;
            decimal targetPosition;

            //INFO Absolute movement using the min and max reachable positions for limits
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (homingDone)
            {
                feedRate = this.verticalManualMovementsDataLayer.FeedRateAfterZero;
                movementType = MovementType.Absolute;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? this.verticalAxisDataLayer.UpperBound
                    : this.verticalAxisDataLayer.LowerBound;
            }
            else //INFO Before homing relative movements step by step
            {
                feedRate = this.verticalManualMovementsDataLayer.FeedRateVM;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? this.verticalManualMovementsDataLayer.PositiveTargetDirection
                    : -this.verticalManualMovementsDataLayer.NegativeTargetDirection;
            }

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * feedRate };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                movementType,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void MoveVerticalOfDistance(decimal distance)
        {
            if (distance == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distance),
                    Resources.Elevator.MovementDistanceCannotBeZero);
            }

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            decimal[] speed = { this.verticalAxisDataLayer.MaxEmptySpeed * this.verticalManualMovementsDataLayer.FeedRateAfterZero };
            decimal[] acceleration = { this.verticalAxisDataLayer.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxisDataLayer.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                distance,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);
        }

        public void UpdateResolution(decimal newResolution)
        {
            if (newResolution <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newResolution));
            }

            this.verticalAxisDataLayer.Resolution = newResolution;

            this.setupStatusProvider.CompleteVerticalResolution();
        }

        private decimal GetFeedRate(FeedRateCategory feedRateCategory)
        {
            decimal feedRate;
            switch (feedRateCategory)
            {
                case FeedRateCategory.VerticalManualMovements:
                    feedRate = this.verticalManualMovementsDataLayer.FeedRateVM;
                    break;

                case FeedRateCategory.VerticalManualMovementsAfterZero:
                    feedRate = this.verticalManualMovementsDataLayer.FeedRateAfterZero;
                    break;

                case FeedRateCategory.HorizontalManualMovements:
                    feedRate = this.horizontalManualMovementsDataLayer.FeedRateHM;
                    break;

                case FeedRateCategory.VerticalResolutionCalibration:
                    feedRate = this.resolutionCalibrationDataLayer.FeedRate;
                    break;

                case FeedRateCategory.VerticalOffsetCalibration:
                    feedRate = this.offsetCalibrationDataLayer.FeedRateOC;
                    break;

                case FeedRateCategory.CellHeightCheck:
                    feedRate = this.cellControlDataLayer.FeedRateCC;
                    break;

                case FeedRateCategory.PanelHeightCheck:
                    feedRate = this.panelControlDataLayer.FeedRatePC;
                    break;

                case FeedRateCategory.LoadingUnitWeight:
                    feedRate = this.weightControl.FeedRateWC;
                    break;

                case FeedRateCategory.BayHeight:
                    feedRate = this.bayPositionControl.FeedRateBP;
                    break;

                case FeedRateCategory.LoadFirstDrawer:
                    throw new NotImplementedException(nameof(FeedRateCategory.LoadFirstDrawer));

                case FeedRateCategory.ShutterManualMovements:
                    throw new NotImplementedException(nameof(FeedRateCategory.ShutterManualMovements));

                case FeedRateCategory.ShutterHeightCheck:
                    throw new NotImplementedException(nameof(FeedRateCategory.ShutterHeightCheck));

                default:
                    throw new ArgumentOutOfRangeException(nameof(feedRateCategory));
            }

            return feedRate;
        }

        #endregion
    }
}
