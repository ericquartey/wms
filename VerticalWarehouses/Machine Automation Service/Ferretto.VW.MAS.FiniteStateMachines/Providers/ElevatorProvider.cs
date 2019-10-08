using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.FiniteStateMachines.Providers
{
    internal sealed class ElevatorProvider : BaseProvider, IElevatorProvider, IDisposable
    {
        #region Fields

        private readonly IBayPositionControlDataLayer bayPositionControl;

        private readonly ICellControlDataLayer cellControlDataLayer;

        private readonly IDepositAndPickUpDataLayer depositAndPickUpDataLayer;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly ILogger<FiniteStateMachines> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IOffsetCalibrationDataLayer offsetCalibrationDataLayer;

        private readonly IPanelControlDataLayer panelControlDataLayer;

        private readonly IResolutionCalibrationDataLayer resolutionCalibrationDataLayer;

        private readonly IServiceScope scope;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalManualMovementsDataLayer verticalManualMovementsDataLayer;

        private readonly IWeightControlDataLayer weightControl;

        private bool disposedValue = false;

        #endregion

        #region Constructors

        public ElevatorProvider(
                    IEventAggregator eventAggregator,
                    ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator)
        {
            this.scope = serviceScopeFactory.CreateScope();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

            this.panelControlDataLayer = this.scope.ServiceProvider.GetRequiredService<IPanelControlDataLayer>();
            this.horizontalManualMovementsDataLayer = this.scope.ServiceProvider.GetRequiredService<IHorizontalManualMovementsDataLayer>();
            this.resolutionCalibrationDataLayer = this.scope.ServiceProvider.GetRequiredService<IResolutionCalibrationDataLayer>();
            this.offsetCalibrationDataLayer = this.scope.ServiceProvider.GetRequiredService<IOffsetCalibrationDataLayer>();
            this.bayPositionControl = this.scope.ServiceProvider.GetRequiredService<IBayPositionControlDataLayer>();
            this.cellControlDataLayer = this.scope.ServiceProvider.GetRequiredService<ICellControlDataLayer>();
            this.setupStatusProvider = this.scope.ServiceProvider.GetRequiredService<ISetupStatusProvider>();
            this.weightControl = this.scope.ServiceProvider.GetRequiredService<IWeightControlDataLayer>();
            this.machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
            this.sensorsProvider = this.scope.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.verticalManualMovementsDataLayer = this.scope.ServiceProvider.GetRequiredService<IVerticalManualMovementsDataLayer>();
            this.loadingUnitsProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsProvider>();
            this.depositAndPickUpDataLayer = this.scope.ServiceProvider.GetRequiredService<IDepositAndPickUpDataLayer>();
        }

        #endregion

        #region Properties

        public double HorizontalPosition { get; set; }

        public double VerticalPosition { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///   This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public int GetDepositAndPickUpCycleQuantity()
        {
            return this.elevatorDataProvider.GetDepositAndPickUpCycleQuantity();
        }

        public void IncreaseDepositAndPickUpCycleQuantity()
        {
            this.elevatorDataProvider.IncreaseDepositAndPickUpCycleQuantity();
        }

        public void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, int? loadingUnitId, double? loadingUnitNetWeight, BayNumber requestingBay, MessageActor sender)
        {
            var sensors = this.sensorsProvider.GetAll();
            this.elevatorDataProvider.SetLoadingUnitOnBoard(loadingUnitId);

            if (loadingUnitId.HasValue
                &&
                loadingUnitNetWeight.HasValue)
            {
                this.loadingUnitsProvider.SetWeight(loadingUnitId.Value, loadingUnitNetWeight.Value);
            }

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

            var profileType = SelectProfileType(direction, isStartedOnBoard);
            this.logger.LogDebug($"MoveHorizontalAuto: ProfileType: {profileType}; HorizontalPosition: {(int)this.HorizontalPosition}");

            var profileSteps = this.elevatorDataProvider.GetHorizontalAxis().Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);

            // if direction is Forwards height increments, else is decremented
            var directionMultiplier = direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = profileSteps.Select(s => s.Speed).ToArray();
            var acceleration = profileSteps.Select(s => s.Acceleration).ToArray();
            var deceleration = profileSteps.Select(s => s.Deceleration).ToArray();
            var switchPosition = profileSteps.Select(s => this.HorizontalPosition + (s.Position * directionMultiplier)).ToArray();

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
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender)
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
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveToVerticalPosition(double targetPosition, FeedRateCategory feedRateCategory, BayNumber bayNumber, MessageActor sender)
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
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void MoveVertical(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender)
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
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void MoveVerticalOfDistance(double distance, BayNumber bayNumber, MessageActor sender, double feedRate = 1)
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
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void RepeatVerticalMovement(double upperBoundPosition, double lowerBoundPosition,
            int totalTestCycleCount, int delayStart, BayNumber bayNumber, MessageActor sender)
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
                sender,
                MessageType.Positioning,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public void ResetDepositAndPickUpCycleQuantity()
        {
            this.elevatorDataProvider.ResetDepositAndPickUpCycleQuantity();
        }

        public void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId,
                    BayNumber requestingBay, MessageActor sender)
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
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void Stop(BayNumber bayNumber, MessageActor sender)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.Stop,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        private static MovementProfileType SelectProfileType(HorizontalMovementDirection direction, bool isStartedOnBoard)
        {
            // the total length is splitted in two unequal distances
            var isLongerDistance =
                (isStartedOnBoard && direction == HorizontalMovementDirection.Forwards)
                ||
                (!isStartedOnBoard && direction == HorizontalMovementDirection.Backwards);

            if (isLongerDistance && isStartedOnBoard)
            {
                return MovementProfileType.LongDeposit;
            }
            else if (isLongerDistance && !isStartedOnBoard)
            {
                return MovementProfileType.LongPickup;
            }
            else if (!isLongerDistance && isStartedOnBoard)
            {
                return MovementProfileType.ShortDeposit;
            }
            else
            {
                return MovementProfileType.ShortPickup;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.scope.Dispose();
                }

                this.disposedValue = true;
            }
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

            var axis = orientation == Orientation.Horizontal
                ? this.elevatorDataProvider.GetHorizontalAxis()
                : this.elevatorDataProvider.GetVerticalAxis();

            if (loadingUnit is null)
            {
                return axis.EmptyLoadMovement;
            }

            var maximumLoadMovement = axis.MaximumLoadMovement;
            var emptyLoadMovement = axis.EmptyLoadMovement;

            var loadingUnitWeight = loadingUnit?.GrossWeight ?? 0;

            var scalingFactor = loadingUnitWeight / this.elevatorDataProvider.GetStructuralProperties().MaximumLoadOnBoard;

            return new MovementParameters
            {
                Speed = emptyLoadMovement.Speed + ((maximumLoadMovement.Speed - emptyLoadMovement.Speed) * scalingFactor),
                Acceleration = emptyLoadMovement.Acceleration + ((maximumLoadMovement.Acceleration - emptyLoadMovement.Acceleration) * scalingFactor),
                Deceleration = emptyLoadMovement.Deceleration + ((maximumLoadMovement.Deceleration - emptyLoadMovement.Deceleration) * scalingFactor),
            };
        }

        #endregion
    }
}
