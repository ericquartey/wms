using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal sealed class ElevatorProvider : BaseProvider, IElevatorProvider
    {
        #region Fields

        private readonly IBayPositionControlDataLayer bayPositionControl;

        private readonly ICellControlDataLayer cellControlDataLayer;

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly IPanelControlDataLayer panelControlDataLayer;

        private readonly IResolutionCalibrationDataLayer resolutionCalibrationDataLayer;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer;

        private readonly IWeightControlDataLayer weightControl;

        #endregion

        #region Constructors

        public ElevatorProvider(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IConfigurationValueManagmentDataLayer configurationProvider,
            IPanelControlDataLayer panelControlDataLayer,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            IResolutionCalibrationDataLayer resolutionCalibrationDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            IBayPositionControlDataLayer bayPositionControl,
            ICellControlDataLayer cellControlDataLayer,
            ISetupStatusProvider setupStatusProvider,
            IWeightControlDataLayer weightControl,
            IMachineProvider machineProvider,
            ISensorsProvider sensorsProvider,
            IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer,
            ILoadingUnitsProvider loadingUnitsProvider)
            : base(eventAggregator)
        {
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.panelControlDataLayer = panelControlDataLayer ?? throw new ArgumentNullException(nameof(panelControlDataLayer));
            this.horizontalManualMovementsDataLayer = horizontalManualMovementsDataLayer ?? throw new ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            this.resolutionCalibrationDataLayer = resolutionCalibrationDataLayer ?? throw new ArgumentNullException(nameof(resolutionCalibrationDataLayer));
            this.offsetCalibrationDataLayer = offsetCalibrationDataLayer ?? throw new ArgumentNullException(nameof(offsetCalibrationDataLayer));
            this.bayPositionControl = bayPositionControl ?? throw new ArgumentNullException(nameof(bayPositionControl));
            this.cellControlDataLayer = cellControlDataLayer ?? throw new ArgumentNullException(nameof(cellControlDataLayer));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
            this.weightControl = weightControl ?? throw new ArgumentNullException(nameof(weightControl));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.verticalManualMovementsDataLayer = verticalManualMovementsDataLayer ?? throw new ArgumentNullException(nameof(verticalManualMovementsDataLayer));
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new ArgumentNullException(nameof(loadingUnitsProvider));
        }

        #endregion

        #region Methods

        public double? GetHorizontalPosition(BayNumber requestingBay)
        {
            var messageData = new RequestPositionMessageData(Axis.Horizontal, 0);

            void PublishAction()
            {
                this.PublishCommand(
                    messageData,
                    "Request Horizontal position",
                    MessageActor.FiniteStateMachines,
                    MessageType.RequestPosition,
                    requestingBay,
                    BayNumber.ElevatorBay);
            }

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                PublishAction);

            return notifyData.CurrentPosition ?? 0;
        }

        public double GetVerticalPosition(BayNumber bayNumber)
        {
            var messageData = new RequestPositionMessageData(Axis.Vertical, 0);

            void PublishAction()
            {
                this.PublishCommand(
                    messageData,
                    "Request vertical position",
                    MessageActor.FiniteStateMachines,
                    MessageType.RequestPosition,
                    bayNumber,
                    BayNumber.ElevatorBay);
            }

            var notifyData = this.WaitForResponseEventAsync<PositioningMessageData>(
                MessageType.Positioning,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting,
                PublishAction);

            return notifyData.CurrentPosition ?? 0;
        }

        public void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, BayNumber requestingBay)
        {
            var sensors = this.sensorsProvider.GetAll(requestingBay);

            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSideBay1]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

            if (isStartedOnBoard != isLoadingUnitOnBoard)
            {
                throw new InvalidOperationException(
                    "Invalid " + (isStartedOnBoard ? "Deposit" : "Pickup") + " command for " + (isStartedOnBoard ? "empty" : "full") + " elevator");
            }

            var zeroSensor = this.machineProvider.IsOneTonMachine()
                ? IOMachineSensors.ZeroPawlSensorOneK
                : IOMachineSensors.ZeroPawlSensor;

            if ((!isStartedOnBoard && !sensors[(int)zeroSensor]) || (isStartedOnBoard && sensors[(int)zeroSensor]))
            {
                throw new InvalidOperationException("Invalid Zero Chain position");
            }

            // execute command
            var position = this.GetHorizontalPosition(requestingBay).Value;

            // if direction is Forwards height increments, else is decremented

            // the total length is splitted in two unequal distances
            var isLongerDistance =
                (isStartedOnBoard && direction == HorizontalMovementDirection.Forwards)
                ||
                (!isStartedOnBoard && direction == HorizontalMovementDirection.Backwards);

            var horizontalAxis = this.elevatorDataProvider.GetHorizontalAxis();
            var profileSteps = horizontalAxis.Profiles
                .Single(p => p.Name == (isLongerDistance ? MovementProfileType.LongProfile : MovementProfileType.ShortProfile))
                .Steps
                .OrderBy(s => s.Number);

            var directionMultiplier = direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = profileSteps.Select(s => s.Speed).ToArray();
            var acceleration = profileSteps.Select(s => s.Acceleration).ToArray();
            var deceleration = profileSteps.Select(s => s.Deceleration).ToArray();
            var switchPosition = profileSteps.Select(s => position + (s.Position * directionMultiplier)).ToArray();

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
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay)
        {
            var setupStatus = this.setupStatusProvider.Get();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? (double)this.horizontalManualMovementsDataLayer.RecoveryTargetPositionHM
                : (double)this.horizontalManualMovementsDataLayer.InitialTargetPositionHM;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * (double)this.horizontalManualMovementsDataLayer.FeedRateHM / 10 };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };

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
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveToVerticalPosition(double targetPosition, FeedRateCategory feedRateCategory, BayNumber bayNumber)
        {
            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();
            var lowerBound = Math.Max(verticalAxis.LowerBound, verticalAxis.Offset);
            var upperBound = verticalAxis.UpperBound;

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

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * feedRate };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };

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
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void MoveVertical(VerticalMovementDirection direction, BayNumber bayNumber)
        {
            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();
            var movementType = MovementType.Relative;

            double feedRate;
            double targetPosition;

            // INFO Absolute movement using the min and max reachable positions for limits
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (homingDone)
            {
                feedRate = (double)this.verticalManualMovementsDataLayer.FeedRateAfterZero;
                movementType = MovementType.Absolute;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? verticalAxis.UpperBound
                    : verticalAxis.LowerBound;
            }

            // INFO Before homing relative movements step by step
            else
            {
                feedRate = (double)this.verticalManualMovementsDataLayer.FeedRateVM;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? (double)this.verticalManualMovementsDataLayer.PositiveTargetDirection
                    : (double)-this.verticalManualMovementsDataLayer.NegativeTargetDirection;
            }

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * feedRate };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };

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
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void MoveVerticalOfDistance(double distance, BayNumber bayNumber, double feedRate = 1)
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

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * feedRate };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };

            var direction = distance > 0
                ? HorizontalMovementDirection.Forwards
                : HorizontalMovementDirection.Backwards;

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
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void RepeatVerticalMovement(double upperBoundPosition, double lowerBoundPosition, int totalTestCycleCount, int delayStart, BayNumber bayNumber)
        {
            if (totalTestCycleCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    Resources.BeltBurnishingProcedure.TheNumberOfTestCyclesMustBeStrictlyPositive,
                    nameof(totalTestCycleCount));
            }

            if (upperBoundPosition <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    Resources.BeltBurnishingProcedure.UpperBoundPositionMustBeStrictlyPositive,
                    nameof(upperBoundPosition));
            }

            if (upperBoundPosition <= lowerBoundPosition)
            {
                throw new ArgumentOutOfRangeException(
                    Resources.BeltBurnishingProcedure.UpperBoundPositionMustBeStrictlyGreaterThanLowerBoundPosition,
                    nameof(lowerBoundPosition));
            }

            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();

            if (upperBoundPosition > verticalAxis.UpperBound)
            {
                throw new ArgumentOutOfRangeException(
                    Resources.BeltBurnishingProcedure.UpperBoundPositionOutOfRange,
                    nameof(upperBoundPosition));
            }

            if (lowerBoundPosition < verticalAxis.LowerBound)
            {
                throw new ArgumentOutOfRangeException(
                    Resources.BeltBurnishingProcedure.LowerBoundPositionOutOfRange,
                    nameof(lowerBoundPosition));
            }

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };

            var data = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.BeltBurnishing,
                upperBoundPosition,
                speed,
                acceleration,
                deceleration,
                totalTestCycleCount,
                lowerBoundPosition,
                upperBoundPosition,
                delayStart,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                data,
                "Execute Belt Burnishing Command",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay)
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

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            double[] speed = { movementParameters.Speed * (double)this.verticalManualMovementsDataLayer.FeedRateAfterZero };
            double[] acceleration = { movementParameters.Acceleration };
            double[] deceleration = { movementParameters.Deceleration };
            double[] switchPosition = { 0 };

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
                LoadingUnitId = loadingUnitId,
            };

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                MessageType.TorqueCurrentSampling,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void Stop(BayNumber bayNumber)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        private double GetFeedRate(FeedRateCategory feedRateCategory)
        {
            double feedRate;
            switch (feedRateCategory)
            {
                case DataModels.FeedRateCategory.VerticalManualMovements:
                    feedRate = (double)this.verticalManualMovementsDataLayer.FeedRateVM;
                    break;

                case DataModels.FeedRateCategory.VerticalManualMovementsAfterZero:
                    feedRate = (double)this.verticalManualMovementsDataLayer.FeedRateAfterZero;
                    break;

                case DataModels.FeedRateCategory.HorizontalManualMovements:
                    feedRate = (double)this.horizontalManualMovementsDataLayer.FeedRateHM;
                    break;

                case DataModels.FeedRateCategory.VerticalResolutionCalibration:
                    feedRate = (double)this.resolutionCalibrationDataLayer.FeedRate;
                    break;

                case DataModels.FeedRateCategory.VerticalOffsetCalibration:
                    feedRate = (double)this.offsetCalibrationDataLayer.FeedRateOC;
                    break;

                case DataModels.FeedRateCategory.CellHeightCheck:
                    feedRate = (double)this.cellControlDataLayer.FeedRateCC;
                    break;

                case DataModels.FeedRateCategory.PanelHeightCheck:
                    feedRate = (double)this.panelControlDataLayer.FeedRatePC;
                    break;

                case DataModels.FeedRateCategory.LoadingUnitWeight:
                    feedRate = (double)this.weightControl.FeedRateWC;
                    break;

                case DataModels.FeedRateCategory.BayHeight:
                    feedRate = (double)this.bayPositionControl.FeedRateBP;
                    break;

                case DataModels.FeedRateCategory.LoadFirstDrawer:
                    throw new NotImplementedException(nameof(DataModels.FeedRateCategory.LoadFirstDrawer));

                case DataModels.FeedRateCategory.ShutterManualMovements:
                    throw new NotImplementedException(nameof(DataModels.FeedRateCategory.ShutterManualMovements));

                case DataModels.FeedRateCategory.ShutterHeightCheck:
                    throw new NotImplementedException(nameof(DataModels.FeedRateCategory.ShutterHeightCheck));

                default:
                    throw new ArgumentOutOfRangeException(nameof(feedRateCategory));
            }

            return feedRate;
        }

        private MovementParameters ScaleMovementsByWeight(Orientation orientation)
        {
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            var loadingUnitWeight = loadingUnit?.GrossWeight ?? 0;

            var scalingFactor = loadingUnitWeight / this.elevatorDataProvider.GetMaximumLoadOnBoard();

            var axis = orientation == Orientation.Horizontal
                ? this.elevatorDataProvider.GetHorizontalAxis()
                : this.elevatorDataProvider.GetVerticalAxis();

            var fullLoadMovement = axis.MaximumLoadMovement;
            var emptyLoadMovement = axis.EmptyLoadMovement;

            return new MovementParameters
            {
                Speed = (fullLoadMovement.Speed - emptyLoadMovement.Speed) * scalingFactor,
                Acceleration = (fullLoadMovement.Acceleration - emptyLoadMovement.Acceleration) * scalingFactor,
                Deceleration = (fullLoadMovement.Deceleration - emptyLoadMovement.Deceleration) * scalingFactor,
            };
        }

        #endregion
    }
}
