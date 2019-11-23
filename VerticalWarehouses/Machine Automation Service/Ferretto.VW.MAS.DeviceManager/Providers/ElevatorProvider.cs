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

        private readonly IBaysProvider baysDataProvider;

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
            IBaysProvider baysDataProvider,
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
            if (!this.IsBayPositionOccupied(bayNumber, bayPositionId))
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
            var shutterPosition = this.machineResourcesProvider.GetShutterPosition(bayNumber);
            if (shutterPosition != ShutterPosition.Opened
                && shutterPosition != ShutterPosition.NotSpecified
                )
            {
                return new ActionPolicy { Reason = Resources.Shutters.TheShutterIsNotCompletelyOpen };
            }

            // check #5: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotFullButThePawlIsNotInZeroPosition };
            }

            return ActionPolicy.Allowed;
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

            // check #4: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotFullButThePawlIsNotInZeroPosition };
            }

            // check #5: the shutters on the same side of the cell must be completely closed
            //           TODO: this is a conservative approach and it could be optimized by inhibiting the operation
            //                 only for cells that are actually obscured by the shutter
            var baysOnSameSide = this.baysDataProvider.GetAll().Where(b => b.Side == cell.Side);
            foreach (var bayOnSameSide in baysOnSameSide)
            {
                if (bayOnSameSide.Shutter != null)
                {
                    var shutterPosition = this.machineResourcesProvider.GetShutterPosition(bayOnSameSide.Number);
                    if (shutterPosition != ShutterPosition.Closed
                        && shutterPosition != ShutterPosition.NotSpecified
                        )
                    {
                        return new ActionPolicy
                        {
                            Reason = string.Format(Resources.Shutters.TheShutterOfBayIsNotCompletelyClosed, (int)bayOnSameSide.Number)
                        };
                    }
                }
            }

            // check #6: the cell's vertical position must be within the elevator's vertical bounds
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            if (cell.Position < verticalAxis.LowerBound || cell.Position > verticalAxis.UpperBound)
            {
                return new ActionPolicy { Reason = Resources.Cells.TheSpecifiedCellIsNotWithinTheElevatorVerticalBounds };
            }

            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanMoveToBayPosition(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: the elevator is already in front of the specified position
            var currentBayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (currentBayPosition?.Id == bayPositionId)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedBayPosition,
                };
            }

            // check #2: the elevator must be empty with pawl in zero position
            //           or
            //           the elevator must be full with pawl in non-zero position
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            var isChainInZeroPosition = this.machineResourcesProvider.IsSensorZeroOnCradle;
            var isElevatorFull = this.machineResourcesProvider.IsDrawerCompletelyOnCradle && loadingUnit != null;
            var isElevatorEmpty = this.machineResourcesProvider.IsDrawerCompletelyOffCradle && loadingUnit is null;

            if (!(isElevatorFull && !isChainInZeroPosition) && !(isElevatorEmpty && isChainInZeroPosition))
            {
                if (!isElevatorEmpty)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.TheElevatorIsNotEmptyButThePawlIsInZeroPosition,
                    };
                }
                else if (!isElevatorFull)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.TheElevatorIsNotFullButThePawlIsNotInZeroPosition,
                    };
                }
            }

            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanMoveToCell(int cellId)
        {
            // check #1: the elevator is already in front of the specified cell
            var elevatorCell = this.elevatorDataProvider.GetCurrentCell();
            if (elevatorCell?.Id == cellId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedCell };
            }

            // check #2: the elevator must be empty with pawl in zero position
            //           or
            //           the elevator must be full with pawl in non-zero position
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            var isChainInZeroPosition = this.machineResourcesProvider.IsSensorZeroOnCradle;
            var isElevatorFull = this.machineResourcesProvider.IsDrawerCompletelyOnCradle && loadingUnit != null;
            var isElevatorEmpty = this.machineResourcesProvider.IsDrawerCompletelyOffCradle && loadingUnit is null;

            if (!(isElevatorFull && !isChainInZeroPosition) && !(isElevatorEmpty && isChainInZeroPosition))
            {
                if (!isElevatorEmpty)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.TheElevatorIsNotEmptyButThePawlIsInZeroPosition,
                    };
                }
                else if (!isElevatorFull)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.TheElevatorIsNotFullButThePawlIsNotInZeroPosition,
                    };
                }
            }

            // check #3: the cell's vertical position must be within the elvator's vertical bounds
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            var targetCell = this.cellsProvider.GetById(cellId);
            if (targetCell.Position < verticalAxis.LowerBound || targetCell.Position > verticalAxis.UpperBound)
            {
                return new ActionPolicy { Reason = Resources.Cells.TheSpecifiedCellIsNotWithinTheElevatorVerticalBounds };
            }

            return ActionPolicy.Allowed;
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
            if (!this.IsBayPositionEmpty(bayNumber, bayPositionId))
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
            var shutterPosition = this.machineResourcesProvider.GetShutterPosition(bayNumber);
            if (shutterPosition != ShutterPosition.Opened
                && shutterPosition != ShutterPosition.NotSpecified
                )
            {
                return new ActionPolicy
                {
                    Reason = Resources.Shutters.TheShutterIsNotCompletelyOpen
                };
            }

            // check #5: elevator's pawl cannot be be in zero position
            if (this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotEmptyButThePawlIsInZeroPosition };
            }

            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanUnloadToCell(int cellId)
        {
            // check #1: elevator must be located opposite to the specified cell
            var elevatorCell = this.elevatorDataProvider.GetCurrentCell();
            if (elevatorCell is null || elevatorCell.Id != cellId)
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
            if (!this.cellsProvider.CanFitLoadingUnit(elevatorCell.Id, loadingUnit.Id))
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheLoadingUnitDoesNotFitInTheSpecifiedCell };
            }

            // check #3: elevator's pawl cannot be be in zero position
            if (this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotEmptyButThePawlIsInZeroPosition };
            }

            // check #4: the shutters on the same side of the cell must be completely closed
            //           TODO: this is a conservative approach and it could be optimized by inhibiting the operation
            //                 only for cells that are actually obscured by the shutter
            var baysOnSameSide = this.baysDataProvider
                .GetAll()
                .Where(b => b.Side == elevatorCell.Side);
            foreach (var bayOnSameSide in baysOnSameSide)
            {
                if (bayOnSameSide.Shutter != null)
                {
                    var shutterPosition = this.machineResourcesProvider.GetShutterPosition(bayOnSameSide.Number);
                    if (shutterPosition != ShutterPosition.Closed
                        && shutterPosition != ShutterPosition.NotSpecified)
                    {
                        return new ActionPolicy
                        {
                            Reason = string.Format(Resources.Shutters.TheShutterOfBayIsNotCompletelyClosed, (int)bayOnSameSide.Number)
                        };
                    }
                }
            }

            // check #5: the cell's vertical position must be within the elvator's vertical bounds
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            if (elevatorCell.Position < verticalAxis.LowerBound || elevatorCell.Position > verticalAxis.UpperBound)
            {
                return new ActionPolicy { Reason = Resources.Cells.TheSpecifiedCellIsNotWithinTheElevatorVerticalBounds };
            }

            return ActionPolicy.Allowed;
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
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

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

            var supposedLoadingUnitGrossWeight = bayPosition.LoadingUnit.MaxNetWeight + bayPosition.LoadingUnit.Tare;

            this.MoveHorizontalAuto(
                direction,
                isLoadingUnitOnBoard: false,
                bayPosition.LoadingUnit.Id,
                supposedLoadingUnitGrossWeight,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender,
                sourceBayPositionId: bayPositionId);
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
                isLoadingUnitOnBoard: false,
                cell.LoadingUnit.Id,
                loadingUnitGrossWeight: null,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender,
                sourceCellId: cellId);
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit.
        /// It uses a Table target movement, mapped by 4 Profiles sets of parameters selected by direction and loading status
        /// </summary>
        /// <param name="direction">Forwards: from elevator to Bay 1 side</param>
        /// <param name="isLoadingUnitOnBoard">true: elevator is full before the movement. It must match the presence sensors</param>
        /// <param name="loadingUnitId">This id is stored in Elevator table before the movement. null means no LoadUnit</param>
        /// <param name="loadingUnitGrossWeight">This weight is stored in LoadingUnits table before the movement.</param>
        /// <param name="waitContinue">true: the inverter positioning state machine stops after the transmission of parameters and waits for a Continue command before enabling inverter</param>
        /// <param name="requestingBay"></param>
        /// <param name="sender"></param>
        public void MoveHorizontalAuto(
            HorizontalMovementDirection direction,
            bool isLoadingUnitOnBoard,
            int? loadingUnitId,
            double? loadingUnitGrossWeight,
            bool waitContinue,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender,
            int? targetCellId = null,
            int? targetBayPositionId = null,
            int? sourceCellId = null,
            int? sourceBayPositionId = null)
        {
            if (loadingUnitId.HasValue
                &&
                loadingUnitGrossWeight.HasValue)
            {
                this.loadingUnitsProvider.SetWeight(loadingUnitId.Value, loadingUnitGrossWeight.Value);
            }

            var sensors = this.sensorsProvider.GetAll();

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

            var profileType = SelectProfileType(direction, isLoadingUnitOnBoard);

            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            var profileSteps = axis.Profiles
                .Single(p => p.Name == profileType)
                .Steps
                .OrderBy(s => s.Number);

            if (!loadingUnitId.HasValue && isLoadingUnitOnBoard)
            {
                var loadUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadUnit != null)
                {
                    loadingUnitId = loadUnit.Id;
                }
            }

            // if weight is unknown we move as full weight
            double scalingFactor = 1;
            if (loadingUnitId.HasValue && !measure)
            {
                var loadUnit = this.loadingUnitsProvider.GetById(loadingUnitId.Value);
                if (loadUnit.MaxNetWeight > 0 && loadUnit.GrossWeight > 0)
                {
                    if (loadUnit.GrossWeight < loadUnit.Tare)
                    {
                        scalingFactor = 0;
                    }
                    else
                    {
                        scalingFactor = (loadUnit.GrossWeight - loadUnit.Tare) / loadUnit.MaxNetWeight;
                    }
                }
            }
            foreach (var profileStep in profileSteps)
            {
                profileStep.ScaleMovementsByWeight(scalingFactor, axis);
            }

            // if direction is Forwards then height increments, otherwise it decrements
            var directionMultiplier = (direction == HorizontalMovementDirection.Forwards ? 1 : -1);

            var speed = profileSteps.Select(s => s.Speed).ToArray();
            var acceleration = profileSteps.Select(s => s.Acceleration).ToArray();
            var deceleration = profileSteps.Select(s => s.Deceleration).ToArray();

            // we use compensation for small errors only (large errors come from new database)
            var compensation = this.HorizontalPosition - axis.LastIdealPosition;
            if (Math.Abs(compensation) > Math.Abs(axis.ChainOffset))
            {
                this.logger.LogWarning($"Do not use compensation for large errors {compensation} > offset {axis.ChainOffset}");
                compensation = 0;
            }
            var switchPosition = profileSteps.Select(s => this.HorizontalPosition - compensation + (s.Position * directionMultiplier)).ToArray();

            var targetPosition = switchPosition.Last();

            this.logger.LogDebug($"MoveHorizontalAuto: ProfileType: {profileType}; " +
                $"HorizontalPosition: {(int)this.HorizontalPosition}; " +
                $"direction: {direction}; " +
                $"measure: {measure}; " +
                $"waitContinue: {waitContinue}; " +
                $"loadUnitId: {loadingUnitId}; " +
                $"scalingFactor: {scalingFactor}; " +
                $"compensation: {compensation}");

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
                waitContinue)
            {
                TargetCellId = targetCellId,
                TargetBayPositionId = targetBayPositionId,
                SourceCellId = sourceCellId,
                SourceBayPositionId = sourceBayPositionId,
            };

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

            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var targetPosition = setupStatus.VerticalOriginCalibration.IsCompleted
                ? axis.ManualMovements.TargetDistanceAfterZero
                : axis.ManualMovements.TargetDistance;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = new[] { axis.FullLoadMovement.Speed * axis.ManualMovements.FeedRate };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
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
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var targetPosition = axis.ManualMovements.TargetDistanceAfterZero;

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = new[] { axis.FullLoadMovement.Speed * axis.ManualMovements.FeedRate };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
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
                    bool manualMovment,
                    double targetPosition,
                    bool computeElongation,
                    bool performWeighting,
                    BayNumber requestingBay,
                    MessageActor sender)
        {
            this.MoveToVerticalPosition(
                performWeighting ? MovementMode.PositionAndMeasure : MovementMode.Position,
                targetPosition,
                manualMovment,
                computeElongation,
                requestingBay,
                sender,
                null,
                null);
        }

        public void MoveToBayPosition(int bayPositionId, bool computeElongation, bool performWeighting, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanMoveToBayPosition(bayPositionId, bayNumber);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            this.MoveToVerticalPosition(
                performWeighting ? MovementMode.PositionAndMeasure : MovementMode.Position,
                bayPosition.Height,
                false,
                computeElongation,
                bayNumber,
                sender,
                bayPositionId,
                targetCellId: null);
        }

        public void MoveToCell(int cellId, bool computeElongation, bool performWeighting, BayNumber requestingBay, MessageActor sender)
        {
            var policy = this.CanMoveToCell(cellId);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var cell = this.cellsProvider.GetById(cellId);

            this.MoveToVerticalPosition(
                performWeighting ? MovementMode.PositionAndMeasure : MovementMode.Position,
                cell.Position,
                false,
                computeElongation,
                requestingBay,
                sender,
                targetBayPositionId: null,
                cellId);
        }

        public void MoveToRelativeVerticalPosition(double distance, BayNumber requestingBay, MessageActor sender)
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

            var manualParameters = this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical);
            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed * manualParameters.FeedRateAfterZero };
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

        public void MoveVerticalManual(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender)
        {
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            var movementType = MovementType.Relative;

            var parameters = verticalAxis.ManualMovements;

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
                targetPosition = parameters.TargetDistance;
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

            var procedureParameters = this.elevatorDataProvider.GetAxis(Orientation.Vertical).WeightMeasurement;

            var manualMovements = this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical);

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

            double[] speed = { procedureParameters.MeasureSpeed };
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
                FeedRate = manualMovements.FeedRate
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

            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

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

            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;

            var assistedMovementsAxis = this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

            var speed = new[] { movementParameters.Speed *
                    (homingDone ? 1 : assistedMovementsAxis.FeedRate) };
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

            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();

            var bay = this.baysDataProvider.GetByNumber(bayNumber);
            var direction = bay.Side is WarehouseSide.Front
                ? HorizontalMovementDirection.Forwards
                : HorizontalMovementDirection.Backwards;

            this.MoveHorizontalAuto(
                direction,
                isLoadingUnitOnBoard: true,
                loadingUnit.Id,
                loadingUnitGrossWeight: null,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender,
                targetBayPositionId: bayPositionId);
        }

        public void UnloadToCell(int cellId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanUnloadToCell(cellId);
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
                isLoadingUnitOnBoard: true,
                loadingUnit.Id,
                loadingUnitGrossWeight: null,
                waitContinue: false,
                measure: false,
                bayNumber,
                sender,
                targetCellId: cellId);
        }

        private static MovementProfileType SelectProfileType(HorizontalMovementDirection direction, bool elevatorHasLoadingUnit)
        {
            // the total length is split in two unequal distances
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

        private bool IsBayPositionEmpty(BayNumber bayNumber, int bayPositionId)
        {
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            var arePresenceSensorsActive = bayPosition.IsUpper
                ? this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                : this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);

            return bayPosition.LoadingUnit is null && !arePresenceSensorsActive;
        }

        private bool IsBayPositionOccupied(BayNumber bayNumber, int bayPositionId)
        {
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);

            var arePresenceSensorsActive = bayPosition.IsUpper
                ? this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                : this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber);

            return bayPosition.LoadingUnit != null && arePresenceSensorsActive;
        }

        private void MoveToVerticalPosition(
            MovementMode movementMode,
            double targetPosition,
            bool manualMovement,
            bool computeElongation,
            BayNumber requestingBay,
            MessageActor sender,
            int? targetBayPositionId,
            int? targetCellId)
        {
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            if (!(verticalAxis.LowerBound <= verticalAxis.Offset
                && verticalAxis.Offset <= verticalAxis.UpperBound)
            )
            {
                throw new ArgumentOutOfRangeException($"Vertical Axis bounds or offset are not valid: lower bound ({verticalAxis.LowerBound}); offset {verticalAxis.Offset}; upper bound {verticalAxis.UpperBound}");
            }

            var lowerBound = verticalAxis.LowerBound;
            var upperBound = verticalAxis.UpperBound;

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetPosition),
                    string.Format(Resources.Elevator.TargetPositionMustBeInRange, targetPosition, lowerBound, upperBound));
            }

            // TODO: DA SOSTITUIRE CON UNO STATO LOGICO
            var homingDone = this.setupStatusProvider.Get().VerticalOriginCalibration.IsCompleted;

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];
            if (movementMode == MovementMode.PositionAndMeasure && !isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not measure weight on empty elevator!");
                movementMode = MovementMode.Position;
            }

            var manualParameters = manualMovement ? this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical) :
                                                    this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);

            // verificare
            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical);

            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var feedRate = homingDone ? manualParameters.FeedRateAfterZero : manualParameters.FeedRate;
            var speed = new[] { movementParameters.Speed * feedRate };

            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                movementMode,
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
                $"MoveToVerticalPosition: {movementMode}; " +
                $"manualMovement: {manualMovement}; " +
                $"targetPosition: {targetPosition}; " +
                $"homing: {homingDone}" +
                $"feedRate: {manualParameters.FeedRate}; " +
                $"speed: {speed[0]}; " +
                $"acceleration: {acceleration[0]}; " +
                $"deceleration: {deceleration[0]}; " +
                $"speed no feedRate: {movementParameters.Speed}; " +
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

        #endregion
    }
}
