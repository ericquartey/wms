﻿using System;
using System.Linq;
using System.Threading;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal sealed class ElevatorProvider : BaseProvider, IElevatorProvider, IDisposable
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly ILogger<DeviceManager> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IServiceScope scope;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        private bool isDisposed = false;

        #endregion

        #region Constructors

        public ElevatorProvider(
                    IEventAggregator eventAggregator,
                    ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator)
        {
            this.scope = serviceScopeFactory.CreateScope();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.setupProceduresDataProvider = this.scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.setupStatusProvider = this.scope.ServiceProvider.GetRequiredService<ISetupStatusProvider>();
            this.machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
            this.sensorsProvider = this.scope.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.loadingUnitsProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsProvider>();
        }

        #endregion

        #region Properties

        public double HorizontalPosition { get; set; }

        public double VerticalPosition { get; set; }

        #endregion

        #region Methods

        public void ContinuePositioning(BayNumber requestingBay, MessageActor sender)
        {
            this.PublishCommand(
                null,
                $"Continue Positioning Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.ContinueMovement,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        /// <summary>
        ///   This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public AxisBounds GetVerticalBounds()
        {
            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();

            return new AxisBounds { Upper = verticalAxis.UpperBound, Lower = verticalAxis.LowerBound };
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit.
        /// It uses a Table target movement, mapped by 4 Profiles sets of parameters selected by direction and loading status
        /// </summary>
        /// <param name="direction">Forwards: from elevator to Bay 1 side</param>
        /// <param name="isStartedOnBoard">true: elevator is full before the movement. It must match the presence sensors</param>
        /// <param name="loadingUnitId">This id is stored in Elevator table before the movement. null means no LoadUnit</param>
        /// <param name="loadingUnitNetWeight">This weight is stored in LoadingUnits table before the movement.</param>
        /// <param name="waitContinue">true: the inverter positioning state machine stops after the transmission of parameters and waits for a Continue command before enabling inverter</param>
        /// <param name="requestingBay"></param>
        /// <param name="sender"></param>
        public void MoveHorizontalAuto(
            HorizontalMovementDirection direction,
            bool isStartedOnBoard,
            int? loadingUnitId,
            double? loadingUnitNetWeight,
            bool waitContinue,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender)
        {
            measure = true;
            var sensors = this.sensorsProvider.GetAll();

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

            if ((!isLoadingUnitOnBoard && !sensors[(int)zeroSensor]) || (isLoadingUnitOnBoard && sensors[(int)zeroSensor]))
            {
                throw new InvalidOperationException("Invalid Zero Chain position");
            }

            if (measure && isLoadingUnitOnBoard)
            {
                this.logger.LogDebug($"Do not measure profile on full elevator!");
                measure = false;
            }

            var profileType = SelectProfileType(direction, isStartedOnBoard);
            this.logger.LogDebug($"MoveHorizontalAuto: ProfileType: {profileType}; HorizontalPosition: {(int)this.HorizontalPosition}");

            var profileSteps = this.elevatorDataProvider.GetHorizontalAxis().Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);

            // if direction is Forwards then height increments, otherwise it decrements
            var directionMultiplier = direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = profileSteps.Select(s => s.Speed).ToArray();
            var acceleration = profileSteps.Select(s => s.Acceleration).ToArray();
            var deceleration = profileSteps.Select(s => s.Deceleration).ToArray();
            var switchPosition = profileSteps.Select(s => this.HorizontalPosition + (s.Position * directionMultiplier)).ToArray();

            var targetPosition = switchPosition.Last();

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                (measure ? MovementMode.PositionAndMeasure : MovementMode.Position),
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction,
                waitContinue);

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

            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalManualMovements();

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? procedureParameters.RecoveryTargetPosition
                : procedureParameters.InitialTargetPosition;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * procedureParameters.FeedRate };
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

        public void MoveToVerticalPosition(double targetPosition, double feedRate, bool measure, BayNumber requestingBay, MessageActor sender)
        {
            if (feedRate <= 0 || feedRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(feedRate));
            }

            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();
            var lowerBound = Math.Max(verticalAxis.LowerBound, verticalAxis.Offset);
            var upperBound = verticalAxis.UpperBound;

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetPosition),
                    string.Format(Resources.Elevator.TargetPositionMustBeInRange, targetPosition, lowerBound, upperBound));
            }

            // TODO remove this check. We can move vertical even if homing is not done: only the feedRate will be smaller!
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (!homingDone)
            {
                throw new InvalidOperationException(
                   Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSideBay1]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSideBay1];
            if (measure && !isLoadingUnitOnBoard)
            {
                this.logger.LogDebug($"Do not measure weight on empty elevator!");
                measure = false;
            }
            var zeroSensor = this.machineProvider.IsOneTonMachine()
                ? IOMachineSensors.ZeroPawlSensorOneK
                : IOMachineSensors.ZeroPawlSensor;

            if ((!isLoadingUnitOnBoard && !sensors[(int)zeroSensor]) || (isLoadingUnitOnBoard && sensors[(int)zeroSensor]))
            {
                throw new InvalidOperationException("Invalid Zero Chain position");
            }

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * feedRate };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };
            this.logger.LogDebug($"MoveToVerticalPosition: {(measure ? MovementMode.PositionAndMeasure : MovementMode.Position)}; " +
                $"targetPosition: {targetPosition}; " +
                $"speed: {speed[0]}; " +
                $"acceleration: {acceleration[0]}; " +
                $"deceleration: {deceleration[0]} ");

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                (measure ? MovementMode.PositionAndMeasure : MovementMode.Position),
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender)
        {
            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();
            var movementType = MovementType.Relative;

            var parameters = this.setupProceduresDataProvider.GetVerticalManualMovements();

            double feedRate;
            double targetPosition;

            // INFO Absolute movement using the min and max reachable positions for limits
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;
            if (homingDone)
            {
                feedRate = parameters.FeedRateAfterZero;
                movementType = MovementType.Absolute;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? verticalAxis.UpperBound
                    : verticalAxis.LowerBound;
            }

            // INFO Before homing relative movements step by step
            else
            {
                feedRate = parameters.FeedRate;

                targetPosition = direction == VerticalMovementDirection.Up
                    ? parameters.PositiveTargetDirection
                    : -parameters.NegativeTargetDirection;
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
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveVerticalOfDistance(double distance, BayNumber requestingBay, MessageActor sender, double feedRate = 1)
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
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void RunTorqueCurrentSampling(
            double displacement,
            double netWeight,
            int? loadingUnitId,
            BayNumber requestingBay,
            MessageActor sender)
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

            var procedureParameters = this.setupProceduresDataProvider.GetWeightCheck();

            var movementParameters = this.ScaleMovementsByWeight(Orientation.Vertical);

            double[] speed = { movementParameters.Speed * procedureParameters.FeedRate };
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

        public void StartBeltBurnishing(
                    double upperBoundPosition,
            double lowerBoundPosition,
            int delayStart,
            BayNumber requestingBay,
            MessageActor sender)
        {
            if (upperBoundPosition <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(upperBoundPosition),
                    Resources.BeltBurnishingProcedure.UpperBoundPositionMustBeStrictlyPositive);
            }

            if (upperBoundPosition <= lowerBoundPosition)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerBoundPosition),
                    Resources.BeltBurnishingProcedure.UpperBoundPositionMustBeStrictlyGreaterThanLowerBoundPosition);
            }

            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();

            if (upperBoundPosition > verticalAxis.UpperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(upperBoundPosition),
                    Resources.BeltBurnishingProcedure.UpperBoundPositionOutOfRange);
            }

            if (lowerBoundPosition < verticalAxis.LowerBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerBoundPosition),
                    Resources.BeltBurnishingProcedure.LowerBoundPositionOutOfRange);
            }

            var procedureParameters = this.setupProceduresDataProvider.GetBeltBurnishingTest();

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
                procedureParameters.RequiredCycles,
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
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void Stop(BayNumber requestingBay, MessageActor sender)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                sender,
                MessageType.Stop,
                requestingBay,
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
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.scope.Dispose();
                }

                this.isDisposed = true;
            }
        }

        private MovementParameters ScaleMovementsByWeight(Orientation orientation)
        {
            var axis = orientation == Orientation.Horizontal
                ? this.elevatorDataProvider.GetHorizontalAxis()
                : this.elevatorDataProvider.GetVerticalAxis();

            var structuralProperties = this.elevatorDataProvider.GetStructuralProperties();

            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();

            return axis.ScaleMovementsByWeight(loadingUnit?.GrossWeight ?? 0, structuralProperties.MaximumLoadOnBoard);
        }

        #endregion
    }
}
