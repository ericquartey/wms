using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
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

        private readonly IHorizontalMovementLongerProfileDataLayer horizontalMovementLongerProfileDataLayer;

        private readonly IHorizontalMovementShorterProfileDataLayer horizontalMovementShorterProfileDataLayer;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly IPanelControlDataLayer panelControlDataLayer;

        private readonly IResolutionCalibrationDataLayer resolutionCalibrationDataLayer;

        private readonly ISensorsProvider sensorsProvider;

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
            IHorizontalMovementShorterProfileDataLayer horizontalMovementShorterProfileDataLayer,
            IHorizontalMovementLongerProfileDataLayer horizontalMovementLongerProfileDataLayer,
            IHorizontalAxisDataLayer horizontalAxisDataLayer,
            IResolutionCalibrationDataLayer resolutionCalibrationDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IBayPositionControlDataLayer bayPositionControl,
            ICellControlDataLayer cellControlDataLayer,
            ISetupStatusProvider setupStatusProvider,
            IWeightControlDataLayer weightControl,
            IMachineConfigurationProvider machineConfigurationProvider,
            ISensorsProvider sensorsProvider,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            ILoadingUnitsProvider loadingUnitsProvider)
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

            if (horizontalMovementShorterProfileDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementShorterProfileDataLayer));
            }

            if (horizontalMovementLongerProfileDataLayer is null)
            {
                throw new ArgumentNullException(nameof(horizontalMovementLongerProfileDataLayer));
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

            if (machineConfigurationProvider is null)
            {
                throw new ArgumentNullException(nameof(machineConfigurationProvider));
            }

            if (sensorsProvider is null)
            {
                throw new ArgumentNullException(nameof(sensorsProvider));
            }

            if (verticalManualMovementsDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            }

            if (loadingUnitsProvider is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsProvider));
            }

            this.dataContext = dataContext;
            this.panelControlDataLayer = panelControlDataLayer;
            this.horizontalManualMovementsDataLayer = horizontalManualMovementsDataLayer;
            this.horizontalMovementShorterProfileDataLayer = horizontalMovementShorterProfileDataLayer;
            this.horizontalMovementLongerProfileDataLayer = horizontalMovementLongerProfileDataLayer;
            this.horizontalAxisDataLayer = horizontalAxisDataLayer;
            this.resolutionCalibrationDataLayer = resolutionCalibrationDataLayer;
            this.offsetCalibrationDataLayer = offsetCalibrationDataLayer;
            this.verticalAxisDataLayer = verticalAxisDataLayer;
            this.bayPositionControl = bayPositionControl;
            this.cellControlDataLayer = cellControlDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.weightControl = weightControl;
            this.machineConfigurationProvider = machineConfigurationProvider;
            this.sensorsProvider = sensorsProvider;
            this.verticalManualMovementsDataLayer = verticalManualMovementsDataLayer;
            this.loadingUnitsProvider = loadingUnitsProvider;
        }

        #endregion

        #region Methods

        public decimal? GetHorizontalPosition()
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, 0);

            void publishAction()
            {
                this.PublishCommand(
                    messageData,
                    "Request Horizontal position",
                    MessageActor.FiniteStateMachines,
                    MessageType.RequestPosition);
            }

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition;
        }

        public decimal? GetVerticalPosition()
        {
            var messageData = new RequestPositionMessageData(Axis.Vertical, 0);

            void publishAction()
            {
                this.PublishCommand(
                messageData,
                "Request vertical position",
                MessageActor.FiniteStateMachines,
                MessageType.RequestPosition);
            }

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                publishAction);

            return notifyData.CurrentPosition;
        }

        public void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard)
        {
            var sensors = this.sensorsProvider.GetAll();

            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSideBay1]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

            if (isStartedOnBoard != isLoadingUnitOnBoard)
            {
                throw new InvalidOperationException(
                    "Invalid " + (isStartedOnBoard ? "Deposit" : "Pickup") + " command for " + (isStartedOnBoard ? "empty" : "full") + " elevator");
            }

            var zeroSensor = this.machineConfigurationProvider.IsOneKMachine()
                ? IOMachineSensors.ZeroPawlSensorOneK
                : IOMachineSensors.ZeroPawlSensor;

            if ((!isStartedOnBoard && !sensors[(int)zeroSensor]) || (isStartedOnBoard && sensors[(int)zeroSensor]))
            {
                throw new InvalidOperationException("Invalid Zero Chain position");
            }

            // execute command
            var position = this.GetHorizontalPosition().Value;

            // if direction is Forwards height increments, else is decremented

            // the total length is splitted in two unequal distances
            var isLongerDistance =
                (isStartedOnBoard && direction == HorizontalMovementDirection.Forwards)
                ||
                (!isStartedOnBoard && direction == HorizontalMovementDirection.Backwards);

            decimal[] speed =
            {
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P0SpeedV1Longer : this.horizontalMovementShorterProfileDataLayer.P0SpeedV1Shorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P1SpeedV2Longer : this.horizontalMovementShorterProfileDataLayer.P1SpeedV2Shorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P2SpeedV3Longer : this.horizontalMovementShorterProfileDataLayer.P2SpeedV3Shorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P3SpeedV4Longer : this.horizontalMovementShorterProfileDataLayer.P3SpeedV4Shorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P4SpeedV5Longer : this.horizontalMovementShorterProfileDataLayer.P4SpeedV5Shorter
            };

            decimal[] acceleration =
            {
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P0AccelerationLonger : this.horizontalMovementShorterProfileDataLayer.P0AccelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P1AccelerationLonger : this.horizontalMovementShorterProfileDataLayer.P1AccelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P2AccelerationLonger : this.horizontalMovementShorterProfileDataLayer.P2AccelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P3AccelerationLonger : this.horizontalMovementShorterProfileDataLayer.P3AccelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P4AccelerationLonger : this.horizontalMovementShorterProfileDataLayer.P4AccelerationShorter
            };

            decimal[] deceleration =
            {
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P0DecelerationLonger : this.horizontalMovementShorterProfileDataLayer.P0DecelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P1DecelerationLonger : this.horizontalMovementShorterProfileDataLayer.P1DecelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P2DecelerationLonger : this.horizontalMovementShorterProfileDataLayer.P2DecelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P3DecelerationLonger : this.horizontalMovementShorterProfileDataLayer.P3DecelerationShorter,
                isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P4DecelerationLonger : this.horizontalMovementShorterProfileDataLayer.P4DecelerationShorter
            };

            var directionMultiplier = direction == HorizontalMovementDirection.Forwards ? 1 : -1;
            decimal[] switchPosition =
            {
                position + (isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P1QuoteLonger : this.horizontalMovementShorterProfileDataLayer.P1QuoteShorter) * directionMultiplier,
                position + (isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P2QuoteLonger : this.horizontalMovementShorterProfileDataLayer.P2QuoteShorter) * directionMultiplier,
                position + (isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P3QuoteLonger : this.horizontalMovementShorterProfileDataLayer.P3QuoteShorter) * directionMultiplier,
                position + (isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P4QuoteLonger : this.horizontalMovementShorterProfileDataLayer.P4QuoteShorter) * directionMultiplier,
                position + (isLongerDistance ? this.horizontalMovementLongerProfileDataLayer.P5QuoteLonger : this.horizontalMovementShorterProfileDataLayer.P5QuoteShorter) * directionMultiplier
            };

            var targetPosition = switchPosition.Last();

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void MoveHorizontalManual(HorizontalMovementDirection direction)
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
                switchPosition,
                direction);

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
                switchPosition,
                HorizontalMovementDirection.Forwards);

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
                switchPosition,
                HorizontalMovementDirection.Forwards);

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
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        public void RunTorqueCurrentSampling(decimal displacement, decimal netWeight, int? loadingUnitId)
        {
            if (displacement <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(displacement),
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
                MovementMode.TorqueCurrentSampling,
                displacement,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition,
                HorizontalMovementDirection.Forwards)
            {
                LoadedNetWeight = netWeight,
                LoadingUnitId = loadingUnitId
            };

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.TorqueCurrentSampling);
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
