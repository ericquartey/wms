using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
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

        private const double policyVerticalTolerance = 0.01;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<ElevatorProvider> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public ElevatorProvider(
            IEventAggregator eventAggregator,
            ILogger<ElevatorProvider> logger,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupStatusProvider setupStatusProvider,
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            IMissionsDataProvider missionsDataProvider,
            ISensorsProvider sensorsProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider)
            : base(eventAggregator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
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

        public ActionPolicy CanExtractFromBay(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: a loading unit must be present in the bay position
            if (!this.IsBayPositionOccupied(bayNumber, bayPositionId))
            {
                return new ActionPolicy { Reason = Resources.Elevator.NoLoadingUnitIsPresentInTheSpecifiedBayPosition };
            }
            // check #2: a loading unit must be waiting to be extracted in the bay position
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);
            if (!this.missionsDataProvider.IsMissionInWaitState(bayNumber, bayPosition.LoadingUnit.Id))
            {
                return new ActionPolicy { Reason = Resources.Elevator.NoMissionIsWaitingInTheSpecifiedBayPosition };
            }
            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanLoadFromBay(int bayPositionId, BayNumber bayNumber, bool isGuided)
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

            // check #4: the shutter must be completely closed or open depending if mission is guided or not
            var shutterInverter = this.baysDataProvider.GetShutterInverterIndex(bayNumber);
            var shutterPosition = this.machineResourcesProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.NotSpecified)
            {
                if (isGuided)
                {
                    if (shutterPosition != ShutterPosition.Closed)
                    {
                        return new ActionPolicy { Reason = Resources.Shutters.TheShutterOfBayIsNotCompletelyClosed };
                    }
                }
                else
                {
                    if (shutterPosition != ShutterPosition.Opened)
                    {
                        return new ActionPolicy { Reason = Resources.Shutters.TheShutterIsNotCompletelyOpen };
                    }
                }
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
            //var baysOnSameSide = this.baysDataProvider.GetAll().Where(b => b.Side == cell.Side);
            //foreach (var bayOnSameSide in baysOnSameSide)
            //{
            //    if (bayOnSameSide.Shutter != null)
            //    {
            //        var shutterPosition = this.machineResourcesProvider.GetShutterPosition(bayOnSameSide.Number);
            //        if (shutterPosition != ShutterPosition.Closed
            //            && shutterPosition != ShutterPosition.NotSpecified)
            //        {
            //            return new ActionPolicy
            //            {
            //                Reason = string.Format(Resources.Shutters.TheShutterOfBayIsNotCompletelyClosed, (int)bayOnSameSide.Number)
            //            };
            //        }
            //    }
            //}

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
            if (currentBayPosition?.Id == bayPositionId && Math.Abs((currentBayPosition?.Id ?? 0f) - this.elevatorDataProvider.VerticalPosition) < policyVerticalTolerance)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedBayPosition,
                    ReasonType = ReasonType.ElevatorInPosition
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
            if (elevatorCell?.Id == cellId && Math.Abs((elevatorCell?.Position ?? 0f) - this.elevatorDataProvider.VerticalPosition) < policyVerticalTolerance)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedCell,
                    ReasonType = ReasonType.ElevatorInPosition
                };
            }

            // check #2: the elevator must be empty with pawl in zero position
            //           or
            //           the elevator must be full with pawl in non-zero position
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            var isChainInZeroPosition = this.machineResourcesProvider.IsSensorZeroOnCradle;
            var isElevatorFull = this.machineResourcesProvider.IsDrawerCompletelyOnCradle; // && loadingUnit != null;
            var isElevatorEmpty = this.machineResourcesProvider.IsDrawerCompletelyOffCradle; // && loadingUnit is null;

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

        public ActionPolicy CanUnloadToBay(int bayPositionId, BayNumber bayNumber, bool isGuided)
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

            // check #4: the shutter must be completely closed or open depending if mission is guided or not
            var shutterInverter = this.baysDataProvider.GetShutterInverterIndex(bayNumber);
            var shutterPosition = this.machineResourcesProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.NotSpecified)
            {
                if (isGuided)
                {
                    if (shutterPosition != ShutterPosition.Closed)
                    {
                        return new ActionPolicy { Reason = Resources.Shutters.TheShutterOfBayIsNotCompletelyClosed };
                    }
                }
                else
                {
                    if (shutterPosition != ShutterPosition.Opened)
                    {
                        return new ActionPolicy { Reason = Resources.Shutters.TheShutterIsNotCompletelyOpen };
                    }
                }
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
            //var baysOnSameSide = this.baysDataProvider
            //    .GetAll()
            //    .Where(b => b.Side == elevatorCell.Side);
            //foreach (var bayOnSameSide in baysOnSameSide)
            //{
            //    if (bayOnSameSide.Shutter != null)
            //    {
            //        var shutterPosition = this.machineResourcesProvider.GetShutterPosition(bayOnSameSide.Number);
            //        if (shutterPosition != ShutterPosition.Closed
            //            && shutterPosition != ShutterPosition.NotSpecified)
            //        {
            //            return new ActionPolicy
            //            {
            //                Reason = string.Format(Resources.Shutters.TheShutterOfBayIsNotCompletelyClosed, (int)bayOnSameSide.Number)
            //            };
            //        }
            //    }
            //}

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

        public void Homing(Axis calibrateAxis, Calibration calibration, int? loadUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender)
        {
            IHomingMessageData homingData = new HomingMessageData(calibrateAxis, calibration, loadUnitId, showErrors);

            this.PublishCommand(
                homingData,
                "Execute Homing Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Homing,
                bayNumber,
                BayNumber.ElevatorBay);
        }

        public bool IsZeroChainSensor()
        {
            var sensors = this.sensorsProvider.GetAll();

            var zeroSensor = this.machineVolatileDataProvider.IsOneTonMachine.Value
                ? IOMachineSensors.ZeroPawlSensorOneTon
                : IOMachineSensors.ZeroPawlSensor;

            return sensors[(int)zeroSensor];
        }

        public void LoadFromBay(int bayPositionId, BayNumber bayNumber, MessageActor sender)
        {
            var policy = this.CanLoadFromBay(bayPositionId, bayNumber, isGuided: false);
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
                measure: true,
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
                targetCellId: cellId,
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
                this.loadingUnitsDataProvider.SetWeight(loadingUnitId.Value, loadingUnitGrossWeight.Value);
            }

            var sensors = this.sensorsProvider.GetAll();

            var zeroSensor = this.machineVolatileDataProvider.IsOneTonMachine.Value
                ? IOMachineSensors.ZeroPawlSensorOneTon
                : IOMachineSensors.ZeroPawlSensor;

            if (!isLoadingUnitOnBoard && !sensors[(int)zeroSensor])
            {
                throw new InvalidOperationException(Resources.Elevator.TheElevatorIsNotFullButThePawlIsNotInZeroPosition);
            }
            if (isLoadingUnitOnBoard && sensors[(int)zeroSensor])
            {
                throw new InvalidOperationException(Resources.Elevator.TheElevatorIsNotEmptyButThePawlIsInZeroPosition);
            }

            if (measure && isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not measure profile on full elevator!");
                measure = false;
            }

            var profileType = this.SelectProfileType(direction, isLoadingUnitOnBoard);

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
                var loadUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId.Value);
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
            var deceleration = profileSteps.Select(s => s.Acceleration).ToArray();

            // we use compensation for small errors only (large errors come from new database)
            var compensation = this.HorizontalPosition - axis.LastIdealPosition;
            if (Math.Abs(compensation) > Math.Abs(axis.ChainOffset))
            {
                this.logger.LogWarning($"Do not use compensation for large errors {compensation:0.2} > offset {axis.ChainOffset}");
                compensation = 0;
            }
            var switchPosition = profileSteps.Select(s => this.HorizontalPosition - compensation + (s.Position * directionMultiplier)).ToArray();

            var targetPosition = switchPosition.Last();

            this.logger.LogInformation($"MoveHorizontalAuto: ProfileType: {profileType}; " +
                $"HorizontalPosition: {(int)this.HorizontalPosition}; " +
                $"direction: {direction}; " +
                $"measure: {measure}; " +
                $"waitContinue: {waitContinue}; " +
                $"loadUnitId: {loadingUnitId}; " +
                $"scalingFactor: {scalingFactor:0.4}; " +
                $"compensation: {compensation}");

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.TableTarget,
                (measure ? MovementMode.PositionAndMeasureProfile : MovementMode.Position),
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

        public void MoveHorizontalManual(HorizontalMovementDirection direction, double distance, bool measure, int? loadingUnitId, int? positionId, BayNumber requestingBay, MessageActor sender)
        {
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var targetPosition = this.machineVolatileDataProvider.IsHomingExecuted
                ? axis.ManualMovements.TargetDistanceAfterZero
                : axis.ManualMovements.TargetDistance;
            if (distance > 0)
            {
                targetPosition = distance;
            }

            targetPosition *= direction == HorizontalMovementDirection.Forwards ? 1 : -1;

            var speed = new[] { axis.FullLoadMovement.Speed * axis.ManualMovements.FeedRate };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
                (measure ? MovementMode.PositionAndMeasureProfile : MovementMode.Position),
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction);

            if (loadingUnitId.HasValue)
            {
                messageData.LoadingUnitId = loadingUnitId;
                messageData.SourceBayPositionId = positionId;
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

        public void MoveHorizontalProfileCalibration(int bayPositionId, BayNumber requestingBay, MessageActor sender)
        {
            var policy = this.CanLoadFromBay(bayPositionId, requestingBay, isGuided: false);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var targetPosition = axis.Profiles.FirstOrDefault().TotalDistance;

            var bay = this.baysDataProvider.GetByNumber(requestingBay);

            var direction = (bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards);

            targetPosition *= (direction == HorizontalMovementDirection.Forwards) ? 1 : -1;

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
                direction)
            {
                SourceBayPositionId = bayPositionId,
            };

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
            int? targetBayPositionId,
            int? targetCellId,
            bool checkHomingDone,
            bool waitContinue,
            bool isPickupMission,
            BayNumber requestingBay,
            MessageActor sender)
        {
            this.MoveToVerticalPosition(
                performWeighting ? MovementMode.PositionAndMeasureWeight : MovementMode.Position,
                targetPosition,
                manualMovment,
                computeElongation,
                requestingBay,
                sender,
                targetBayPositionId,
                targetCellId,
                checkHomingDone,
                waitContinue,
                isPickupMission);
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
                performWeighting ? MovementMode.PositionAndMeasureWeight : MovementMode.Position,
                bayPosition.Height,
                manualMovement: false,
                computeElongation,
                bayNumber,
                sender,
                bayPositionId,
                targetCellId: null,
                checkHomingDone: true,
                waitContinue: false);
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
                performWeighting ? MovementMode.PositionAndMeasureWeight : MovementMode.Position,
                cell.Position,
                manualMovement: false,
                computeElongation,
                requestingBay,
                sender,
                targetBayPositionId: null,
                cellId,
                checkHomingDone: true,
                waitContinue: false);
        }

        public void MoveToFreeCell(int loadUnitId, bool computeElongation, bool performWeighting, BayNumber requestingBay, MessageActor sender)
        {
            int cellId = this.cellsProvider.FindEmptyCell(loadUnitId);
            var policy = this.CanMoveToCell(cellId);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            var cell = this.cellsProvider.GetById(cellId);

            this.MoveToVerticalPosition(
                performWeighting ? MovementMode.PositionAndMeasureWeight : MovementMode.Position,
                cell.Position,
                manualMovement: false,
                computeElongation,
                requestingBay,
                sender,
                targetBayPositionId: null,
                cellId,
                checkHomingDone: true,
                waitContinue: false);
        }

        public void MoveToRelativeVerticalPosition(double distance, BayNumber requestingBay, MessageActor sender)
        {
            if (distance == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distance),
                    Resources.Elevator.MovementDistanceCannotBeZero);
            }

            var homingDone = this.machineVolatileDataProvider.IsHomingExecuted;
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            var manualParameters = this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical);
            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical, isLoadingUnitOnBoard);

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
            var homingDone = this.machineVolatileDataProvider.IsHomingExecuted;
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
                targetPosition = parameters.TargetDistance * (direction == VerticalMovementDirection.Up ? 1 : -1);
            }

            var speed = new[] { verticalAxis.FullLoadMovement.Speed * feedRate };
            var acceleration = new[] { verticalAxis.FullLoadMovement.Acceleration };
            var deceleration = new[] { verticalAxis.FullLoadMovement.Deceleration };
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

        public void ResetBeltBurnishing()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBeltBurnishingTest();

            this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters);
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

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            var homingDone = this.machineVolatileDataProvider.IsHomingExecuted;
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.VerticalOriginCalibrationMustBePerformed);
            }

            var procedureParameters = this.elevatorDataProvider.GetAxis(Orientation.Vertical).WeightMeasurement;

            var manualMovements = this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical);

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical, isLoadingUnitOnBoard);

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

        public MovementProfileType SelectProfileType(HorizontalMovementDirection direction, bool elevatorHasLoadingUnit)
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

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            var procedureParameters = this.setupProceduresDataProvider.GetBeltBurnishingTest();

            var homingDone = this.machineVolatileDataProvider.IsHomingExecuted;

            var assistedMovementsAxis = this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);

            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical, isLoadingUnitOnBoard);

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
            var policy = this.CanUnloadToBay(bayPositionId, bayNumber, isGuided: false);
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
            int? targetCellId,
            bool checkHomingDone,
            bool waitContinue,
            bool isPickupMission = false)
        {
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            var lowerBound = verticalAxis.LowerBound;
            var upperBound = verticalAxis.UpperBound;
            if (verticalAxis.Offset < lowerBound || verticalAxis.Offset > upperBound)
            {
                throw new InvalidOperationException(string.Format(Resources.Elevator.OffsetOutOfBounds, verticalAxis.Offset, lowerBound, upperBound));
            }

            if (targetPosition < lowerBound || targetPosition > upperBound)
            {
                throw new InvalidOperationException(string.Format(Resources.Elevator.TargetPositionOutOfBounds, targetPosition, lowerBound, upperBound));
            }

            var homingDone = (checkHomingDone ? this.machineVolatileDataProvider.IsHomingExecuted : true);

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];
            if (movementMode == MovementMode.PositionAndMeasureWeight && !isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not measure weight on empty elevator!");
                movementMode = MovementMode.Position;
            }
            if (computeElongation && !isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not compute elongation on empty elevator!");
                computeElongation = false;
            }
            if (isPickupMission && isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not add pickup offset on full elevator!");
                isPickupMission = false;
            }

            var manualParameters = manualMovement ? this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical) :
                                                    this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);

            // verificare
            var movementParameters = this.elevatorDataProvider.ScaleMovementsByWeight(Orientation.Vertical, isLoadingUnitOnBoard);

            var acceleration = new[] { movementParameters.Acceleration };
            var deceleration = new[] { movementParameters.Deceleration };
            var feedRate = homingDone ? manualParameters.FeedRateAfterZero : manualParameters.FeedRate;
            var speed = new[] { movementParameters.Speed * (sender == MessageActor.AutomationService ? feedRate : 1) };

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
                FeedRate = (sender == MessageActor.AutomationService ? feedRate : 1),
                ComputeElongation = computeElongation,
                TargetBayPositionId = targetBayPositionId,
                TargetCellId = targetCellId,
                WaitContinue = waitContinue,
                IsPickupMission = isPickupMission
            };

            this.logger.LogInformation(
                $"MoveToVerticalPosition: {movementMode}; " +
                $"manualMovement: {manualMovement}; " +
                $"targetPosition: {targetPosition}; " +
                $"homing: {homingDone}; " +
                $"feedRate: {(sender == MessageActor.AutomationService ? feedRate : 1)}; " +
                $"speed: {speed[0]:0.2}; " +
                $"acceleration: {acceleration[0]:0.2}; " +
                $"deceleration: {deceleration[0]:0.2}; " +
                $"speed w/o feedRate: {movementParameters.Speed:0.2}; " +
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
