using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal sealed class ElevatorProvider : BaseProvider, IElevatorProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly ILogger<DeviceManagerService> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public ElevatorProvider(
            IEventAggregator eventAggregator,
            ILogger<DeviceManagerService> logger,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupStatusProvider setupStatusProvider,
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            IMachineProvider machineProvider,
            IMachineResourcesProvider machineResourcesProvider,
            ISensorsProvider sensorsProvider,
            ILoadingUnitsProvider loadingUnitsProvider)
            : base(eventAggregator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new ArgumentNullException(nameof(loadingUnitsProvider));
        }

        #endregion

        #region Properties

        public double HorizontalPosition
        {
            get => this.elevatorDataProvider.HorizontalPosition;
            set => this.elevatorDataProvider.HorizontalPosition = value;
        }

        public double VerticalPosition
        {
            get => this.elevatorDataProvider.VerticalPosition;
            set => this.elevatorDataProvider.VerticalPosition = value;
        }

        #endregion

        #region Methods

        public ActionPolicy CanLoadFromBay(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified bay position
            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (bayPosition?.Id != bayPositionId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition };
            }

            // check #2: a loading unit must be present in the bay position
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var isUpperPosition = bayPosition.Height == bay.Positions.Max(p => p.Height);

            var arePresenceSensorsActive = isUpperPosition
                ? this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                : this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);

            if (bayPosition.LoadingUnit is null
                ||
                !arePresenceSensorsActive)
            {
                return new ActionPolicy { Reason = Resources.Elevator.NoLoadingUnitIsPresentInTheSpecifiedBayPosition };
            }

            // check #3: no loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ALoadingUnitIsAlreadyOnBoardOfTheElevator };
            }

            // check #4: the shutter must be completely open
            if (this.machineResourcesProvider.GetShutterPosition(bayNumber) != ShutterPosition.Opened)
            {
                return new ActionPolicy { Reason = Resources.Shutters.TheShutterIsNotCompletelyOpen };
            }

            return new ActionPolicy { IsAllowed = true };
        }

        public ActionPolicy CanLoadFromCell(int cellId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified cell
            var cell = this.elevatorDataProvider.GetCurrentCell();
            if (cell?.Id != cellId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotLocatedOppositeToTheSpecifiedCell };
            }

            // check #2: a loading unit must be present in the cell
            if (cell.LoadingUnit is null)
            {
                return new ActionPolicy { Reason = Resources.Elevator.NoLoadingUnitIsPresentInTheSpecifiedCell };
            }

            // check #3: no loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ALoadingUnitIsAlreadyOnBoardOfTheElevator };
            }

            return new ActionPolicy { IsAllowed = true };
        }

        public ActionPolicy CanUnloadToBay(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified bay position
            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (bayPosition?.Id != bayPositionId)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition
                };
            }

            // check #2: the bay position must not contain a loading unit
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var isUpperPosition = bayPosition.Height == bay.Positions.Max(p => p.Height);

            var arePresenceSensorsActive = isUpperPosition
                ? this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                : this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);

            if (bayPosition.LoadingUnit != null
                ||
                arePresenceSensorsActive)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ALoadingUnitIsAlreadyPresentInTheSpecifiedBayPosition
                };
            }

            // check #3: a loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() is null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOnCradle)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.NoLoadingUnitIsOnBoardOfTheElevator
                };
            }

            // check #4: the shutter must be completely open
            if (this.machineResourcesProvider.GetShutterPosition(bayNumber) != ShutterPosition.Opened)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Shutters.TheShutterIsNotCompletelyOpen
                };
            }

            return new ActionPolicy { IsAllowed = true };
        }

        public ActionPolicy CanUnloadToCell(int cellId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified cell
            var cell = this.elevatorDataProvider.GetCurrentCell();
            if (cell?.Id != cellId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotLocatedOppositeToTheSpecifiedCell };
            }

            // check #2: a loading unit must be on board of the elevator
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            if (loadingUnit is null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.NoLoadingUnitIsOnBoardOfTheElevator };
            }

            // check #2: there is enough space to host the loading unit in the specified cell
            if (!this.cellsProvider.CanFitLoadingUnit(cell.Id, loadingUnit.Id))
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheLoadingUnitDoesNotFitInTheSpecifiedCell };
            }

            return new ActionPolicy { IsAllowed = true };
        }

        public void ContinuePositioning(BayNumber requestingBay, MessageActor sender)
        {
            this.PublishCommand(
                null,
                $"Continue Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.ContinueMovement,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public AxisBounds GetVerticalBounds()
        {
            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();

            return new AxisBounds { Upper = verticalAxis.UpperBound, Lower = verticalAxis.LowerBound };
        }

        public void LoadFromBay(int bayPositionId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanLoadFromBay(bayPositionId, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            var bay = this.baysDataProvider.GetByNumber(bayNumber);

            var direction = bay.Side is WarehouseSide.Front
                ? HorizontalMovementDirection.Backwards
                : HorizontalMovementDirection.Forwards;

            var supposedLoadingUnitNetWeight = bayPosition.LoadingUnit.MaxNetWeight;

            this.MoveHorizontalAuto(
                direction,
                elevatorHasLoadingUnit: false,
                bayPosition.LoadingUnit.Id,
                supposedLoadingUnitNetWeight,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender);
        }

        public void LoadFromCell(int cellId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanLoadFromCell(cellId, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var cell = this.cellsProvider.GetById(cellId);

            var direction = cell.Side is WarehouseSide.Front
                ? HorizontalMovementDirection.Backwards
                : HorizontalMovementDirection.Forwards;

            this.MoveHorizontalAuto(
                direction,
                elevatorHasLoadingUnit: false,
                cell.LoadingUnit.Id,
                cell.LoadingUnit.NetWeight,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender);
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit.
        /// It uses a Table target movement, mapped by 4 Profiles sets of parameters selected by direction and loading status
        /// </summary>
        /// <param name="direction">Forwards: from elevator to Bay 1 side</param>
        /// <param name="elevatorHasLoadingUnit">true: elevator is full before the movement. It must match the presence sensors</param>
        /// <param name="loadingUnitId">This id is stored in Elevator table before the movement. null means no LoadUnit</param>
        /// <param name="loadingUnitNetWeight">This weight is stored in LoadingUnits table before the movement.</param>
        /// <param name="waitContinue">true: the inverter positioning state machine stops after the transmission of parameters and waits for a Continue command before enabling inverter</param>
        /// <param name="requestingBay"></param>
        /// <param name="sender"></param>
        public void MoveHorizontalAuto(
            HorizontalMovementDirection direction,
            bool elevatorHasLoadingUnit,
            int? loadingUnitId,
            double? loadingUnitNetWeight,
            bool waitContinue,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender)
        {
            var sensors = this.sensorsProvider.GetAll();

            if (loadingUnitId.HasValue
                &&
                loadingUnitNetWeight.HasValue)
            {
                this.loadingUnitsProvider.SetWeight(loadingUnitId.Value, loadingUnitNetWeight.Value);
            }

            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            if (elevatorHasLoadingUnit != isLoadingUnitOnBoard)
            {
                throw new InvalidOperationException(
                    "Invalid " + (elevatorHasLoadingUnit ? "Deposit" : "Pickup") + " command for " + (elevatorHasLoadingUnit ? "empty" : "full") + " elevator");
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
                this.logger.LogWarning($"Do not measure profile on full elevator!");
                measure = false;
            }

            var profileType = SelectProfileType(direction, elevatorHasLoadingUnit);
            this.logger.LogDebug($"MoveHorizontalAuto: ProfileType: {profileType}; HorizontalPosition: {(int)this.HorizontalPosition}; direction: {direction}; measure: {measure}; waitContinue: {waitContinue}");

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

            if (loadingUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadingUnitId;
            }

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
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

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Horizontal);

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
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveHorizontalProfileCalibration(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender)
        {
            var procedureParameters = this.elevatorDataProvider.GetHorizontalAxis();

            var targetPosition = procedureParameters.ProfileCalibrateLength;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Horizontal);

            var speed = new[] { procedureParameters.ProfileCalibrateSpeed };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                MovementMode.ProfileCalibration,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Profile Calibration Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveToAbsoluteVerticalPosition(
                    double targetPosition,
                    double feedRate,
                    bool measure,
                    bool computeElongation,
                    BayNumber requestingBay,
                    MessageActor sender)
        {
            this.MoveToVerticalPosition(targetPosition, feedRate, measure, computeElongation, requestingBay, sender, null, null);
        }

        public void MoveToBayPosition(int bayPositionId, double feedRate, bool computeElongation, BayNumber requestingBay, MessageActor sender)
        {
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            this.MoveToVerticalPosition(
                bayPosition.Height,
                feedRate,
                measure: false,
                computeElongation,
                requestingBay,
                sender,
                bayPositionId,
                targetCellId: null);
        }

        public void MoveToCell(int cellId, double feedRate, bool computeElongation, BayNumber requestingBay, MessageActor sender)
        {
            var cell = this.cellsProvider.GetById(cellId);

            this.MoveToVerticalPosition(
                cell.Position,
                feedRate,
                measure: false,
                computeElongation,
                requestingBay,
                sender,
                targetBayPositionId: null,
                cellId);
        }

        public void MoveToRelativeVerticalPosition(double distance, BayNumber requestingBay, MessageActor sender, double feedRate = 1)
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

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

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

            this.logger.LogDebug($"MoveVerticalOfDistance: " +
                $"distance: {distance}; " +
                $"speed: {speed[0]}; " +
                $"acceleration: {acceleration[0]}; " +
                $"deceleration: {deceleration[0]}; ");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void MoveToVerticalPosition(
                    double targetPosition,
                    double feedRate,
                    bool measure,
                    bool computeElongation,
                    BayNumber requestingBay,
                    MessageActor sender,
                    int? targetBayPositionId,
                    int? targetCellId)
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

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];
            if (measure && !isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not measure weight on empty elevator!");
                measure = false;
            }

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * feedRate };
            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var switchPosition = new[] { 0.0 };
            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                measure ? MovementMode.PositionAndMeasure : MovementMode.Position,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Forwards)
            {
                LoadingUnitId = this.elevatorDataProvider.GetLoadingUnitOnBoard()?.Id,
                FeedRate = feedRate,
                ComputeElongation = computeElongation,
                TargetBayPositionId = targetBayPositionId,
                TargetCellId = targetCellId,
            };

            this.logger.LogDebug(
                $"MoveToVerticalPosition: {(measure ? MovementMode.PositionAndMeasure : MovementMode.Position)}; " +
                $"targetPosition: {targetPosition}; " +
                $"speed: {speed[0]}; " +
                $"acceleration: {acceleration[0]}; " +
                $"deceleration: {deceleration[0]}; " +
                $"LU id: {messageData.LoadingUnitId.GetValueOrDefault()}");

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.DeviceManager,
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

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

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
                MessageActor.DeviceManager,
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

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

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
                FeedRate = procedureParameters.FeedRate
            };

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.DeviceManager,
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

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

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
                MessageActor.DeviceManager,
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
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void UnloadToBay(int bayPositionId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanUnloadToBay(bayPositionId, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var currentBayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            var direction = bay.Side is WarehouseSide.Front
                ? HorizontalMovementDirection.Forwards
                : HorizontalMovementDirection.Backwards;

            this.MoveHorizontalAuto(
                direction,
                elevatorHasLoadingUnit: true,
                currentBayPosition.LoadingUnit.Id,
                currentBayPosition.LoadingUnit.NetWeight,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender);
        }

        public void UnloadToCell(int cellId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanUnloadToCell(cellId, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();

            var cell = this.cellsProvider.GetById(cellId);
            var direction = cell.Side is WarehouseSide.Front
               ? HorizontalMovementDirection.Forwards
               : HorizontalMovementDirection.Backwards;

            this.MoveHorizontalAuto(
                direction,
                elevatorHasLoadingUnit: true,
                loadingUnit.Id,
                loadingUnit.NetWeight,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender);
        }

        private static MovementProfileType SelectProfileType(HorizontalMovementDirection direction, bool elevatorHasLoadingUnit)
        {
            // the total length is splitted in two unequal distances
            var isLongerDistance =
                (elevatorHasLoadingUnit && direction == HorizontalMovementDirection.Forwards)
                ||
                (!elevatorHasLoadingUnit && direction == HorizontalMovementDirection.Backwards);

            if (isLongerDistance && elevatorHasLoadingUnit)
            {
                return MovementProfileType.LongDeposit;
            }
            else if (isLongerDistance && !elevatorHasLoadingUnit)
            {
                return MovementProfileType.LongPickup;
            }
            else if (!isLongerDistance && elevatorHasLoadingUnit)
            {
                return MovementProfileType.ShortDeposit;
            }
            else
            {
                return MovementProfileType.ShortPickup;
            }
        }

        #endregion
    }
}
