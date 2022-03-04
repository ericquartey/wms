using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal sealed partial class ElevatorProvider : BaseProvider, IElevatorProvider
    {
        #region Fields

        private const double policyVerticalTolerance = 0.01;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IInvertersProvider invertersProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<ElevatorProvider> logger;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IShutterProvider shutterProvider;

        #endregion

        #region Constructors

        public ElevatorProvider(
            IEventAggregator eventAggregator,
            ILogger<ElevatorProvider> logger,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IErrorsProvider errorsProvider,
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            IMissionsDataProvider missionsDataProvider,
            ISensorsProvider sensorsProvider,
            IShutterProvider shutterProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IInvertersProvider invertersProvider)
            : base(eventAggregator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.shutterProvider = shutterProvider ?? throw new ArgumentNullException(nameof(shutterProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.invertersProvider = invertersProvider ?? throw new ArgumentNullException(nameof(invertersProvider));
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

        public ActionPolicy CanCalibrateHorizontal(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified bay position
            var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            if (bayPosition?.Id != bayPositionId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition", CommonUtils.Culture.Actual) };
            }

            var cellId = this.cellsProvider.GetAll().FirstOrDefault(c => Math.Abs(bayPosition.Height - c.Position) < 25 && !c.IsFree)?.Id;

            if (cellId != null)
            {
                return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheCellIsNotFree", CommonUtils.Culture.Actual) };
            }

            // check #2: a loading unit must not be present in the bay position
            if (this.IsBayPositionOccupied(bayNumber, bayPositionId))
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyPresentInTheSpecifiedBayPosition", CommonUtils.Culture.Actual) };
            }

            // check #3: no loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyOnBoardOfTheElevator", CommonUtils.Culture.Actual) };
            }

            // check #4: the shutter must be completely open
            var shutterInverter = this.baysDataProvider.GetShutterInverterIndex(bayNumber);
            var shutterPosition = this.machineResourcesProvider.GetShutterPosition(shutterInverter);
            if (shutterPosition != ShutterPosition.NotSpecified)
            {
                if (shutterPosition != ShutterPosition.Opened)
                {
                    return new ActionPolicy { Reason = Resources.Shutters.ResourceManager.GetString("TheShutterIsNotCompletelyOpen", CommonUtils.Culture.Actual) };
                }
            }

            // check #5: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual) };
            }

            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanCalibrateZeroPlate()
        {
            // check #3: no loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyOnBoardOfTheElevator", CommonUtils.Culture.Actual) };
            }

            // check #5: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual) };
            }

            return ActionPolicy.Allowed;
        }

        public MachineErrorCode CanElevatorDeposit(BayPosition bayPosition)
        {
            var bayNumber = bayPosition.Bay.Number;
            var returnValue = MachineErrorCode.NoError;
            if (bayPosition.IsUpper
                && this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                )
            {
                returnValue = MachineErrorCode.TopLevelBayOccupied;
            }
            else if (!bayPosition.IsUpper
                && this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber)
                )
            {
                returnValue = MachineErrorCode.BottomLevelBayOccupied;
            }
            return returnValue;
        }

        public MachineErrorCode CanElevatorPickup(BayPosition bayPosition)
        {
            var bayNumber = bayPosition.Bay.Number;
            var returnValue = MachineErrorCode.NoError;
            if (bayPosition.IsUpper
                && !this.machineResourcesProvider.IsDrawerInBayTop(bayNumber)
                )
            {
                returnValue = MachineErrorCode.TopLevelBayEmpty;
            }
            else if (!bayPosition.IsUpper
                && !this.machineResourcesProvider.IsDrawerInBayBottom(bayNumber)
                )
            {
                returnValue = MachineErrorCode.BottomLevelBayEmpty;
            }
            return returnValue;
        }

        public ActionPolicy CanExtractFromBay(int bayPositionId, BayNumber bayNumber)
        {
            // check #1: a loading unit must be present in the bay position
            if (!this.IsBayPositionOccupied(bayNumber, bayPositionId))
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("NoLoadingUnitIsPresentInTheSpecifiedBayPosition", CommonUtils.Culture.Actual) };
            }
            // check #2: a loading unit must be waiting to be extracted in the bay position
            var bayPosition = this.baysDataProvider.GetPositionById(bayPositionId);
            if (!this.missionsDataProvider.IsMissionInWaitState(bayNumber, bayPosition.LoadingUnit.Id))
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("NoMissionIsWaitingInTheSpecifiedBayPosition", CommonUtils.Culture.Actual) };
            }
            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanLoadFromBay(int bayPositionId, BayNumber bayNumber, bool isGuided, bool enforceLoadUnitPresent = true)
        {
            // check #1: elevator must be located opposite to the specified bay position
            //var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
            //if (bayPosition?.Id != bayPositionId)
            //{
            //    return new ActionPolicy { Reason = Resources.Elevator.TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition };
            //}

            // check #2: a loading unit must be present in the bay position
            if (enforceLoadUnitPresent && !this.IsBayPositionOccupied(bayNumber, bayPositionId))
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("NoLoadingUnitIsPresentInTheSpecifiedBayPosition", CommonUtils.Culture.Actual) };
            }

            // check #3: no loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyOnBoardOfTheElevator", CommonUtils.Culture.Actual) };
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
                        return new ActionPolicy { Reason = Resources.Shutters.ResourceManager.GetString("TheShutterOfBayIsNotCompletelyClosed", CommonUtils.Culture.Actual) };
                    }
                }
                else
                {
                    if (shutterPosition != ShutterPosition.Opened)
                    {
                        return new ActionPolicy { Reason = Resources.Shutters.ResourceManager.GetString("TheShutterIsNotCompletelyOpen", CommonUtils.Culture.Actual) };
                    }
                }
            }

            // check #5: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual) };
            }

            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanLoadFromCell(int cellId, BayNumber bayNumber)
        {
            // check #1: elevator must be located opposite to the specified cell
            var cell = this.elevatorDataProvider.GetCurrentCell();
            if (cell?.Id != cellId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotLocatedOppositeToTheSpecifiedCell", CommonUtils.Culture.Actual) };
            }

            // check #2: a loading unit must be present in the cell
            if (cell.LoadingUnit is null)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("NoLoadingUnitIsPresentInTheSpecifiedCell", CommonUtils.Culture.Actual) };
            }

            // check #3: no loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() != null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyOnBoardOfTheElevator", CommonUtils.Culture.Actual) };
            }

            // check #4: elevator's pawl must be in zero position
            if (!this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual) };
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
                return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellIsNotWithinTheElevatorVerticalBounds", CommonUtils.Culture.Actual) };
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
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedBayPosition", CommonUtils.Culture.Actual),
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
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
                else if (!isElevatorFull)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual),
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
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsAlreadyLocatedOppositeToTheSpecifiedCell", CommonUtils.Culture.Actual),
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
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
                else if (!isElevatorFull)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
            }

            // check #3: the cell's vertical position must be within the elvator's vertical bounds
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            var targetCell = this.cellsProvider.GetById(cellId);
            if (targetCell.Position < verticalAxis.LowerBound || targetCell.Position > verticalAxis.UpperBound)
            {
                return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellIsNotWithinTheElevatorVerticalBounds", CommonUtils.Culture.Actual) };
            }

            // check the load unit profile
            if (loadingUnit != null)
            {
                var lastCell = this.cellsProvider.GetAll().LastOrDefault(c => c.Side == targetCell.Side);
                if (lastCell.Position + 25 < targetCell.Position + loadingUnit.Height)
                {
                    return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellCannotFitTheLoadUnit", CommonUtils.Culture.Actual) };
                }
            }
            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanMoveToHeight(double height)
        {
            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
            if (height < verticalAxis.LowerBound || height > verticalAxis.UpperBound)
            {
                return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedHeightIsNotWithinTheElevatorVerticalBounds", CommonUtils.Culture.Actual) };
            }

            var isChainInZeroPosition = this.machineResourcesProvider.IsSensorZeroOnCradle;
            var isElevatorFull = this.machineResourcesProvider.IsDrawerCompletelyOnCradle; // && loadingUnit != null;
            var isElevatorEmpty = this.machineResourcesProvider.IsDrawerCompletelyOffCradle; // && loadingUnit is null;
            if (!(isElevatorFull && !isChainInZeroPosition) && !(isElevatorEmpty && isChainInZeroPosition))
            {
                if (!isElevatorEmpty)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
                else if (!isElevatorFull)
                {
                    return new ActionPolicy
                    {
                        Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotFullButThePawlIsNotInZeroPosition", CommonUtils.Culture.Actual),
                    };
                }
            }

            // check the load unit profile
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            if (loadingUnit != null)
            {
                var lastCell = this.cellsProvider.GetAll().LastOrDefault();
                if (lastCell.Position + 25 < height + loadingUnit.Height)
                {
                    return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellCannotFitTheLoadUnit", CommonUtils.Culture.Actual) };
                }
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
                    Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotLocatedOppositeToTheSpecifiedBayPosition", CommonUtils.Culture.Actual)
                };
            }

            // check #2: the bay position must not contain a loading unit
            if (!this.IsBayPositionEmpty(bayNumber, bayPositionId))
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("ALoadingUnitIsAlreadyPresentInTheSpecifiedBayPosition", CommonUtils.Culture.Actual)
                };
            }

            // check #3: a loading unit must be on board of the elevator
            if (this.elevatorDataProvider.GetLoadingUnitOnBoard() is null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOnCradle)
            {
                return new ActionPolicy
                {
                    Reason = Resources.Elevator.ResourceManager.GetString("NoLoadingUnitIsOnBoardOfTheElevator", CommonUtils.Culture.Actual)
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
                        return new ActionPolicy { Reason = Resources.Shutters.ResourceManager.GetString("TheShutterOfBayIsNotCompletelyClosed", CommonUtils.Culture.Actual) };
                    }
                }
                else
                {
                    if (shutterPosition != ShutterPosition.Opened)
                    {
                        return new ActionPolicy { Reason = Resources.Shutters.ResourceManager.GetString("TheShutterIsNotCompletelyOpen", CommonUtils.Culture.Actual) };
                    }
                }
            }

            // check #5: elevator's pawl cannot be be in zero position
            if (this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual) };
            }

            return ActionPolicy.Allowed;
        }

        public ActionPolicy CanUnloadToCell(int cellId)
        {
            // check #1: elevator must be located opposite to the specified cell
            var elevatorCell = this.elevatorDataProvider.GetCurrentCell();
            if (elevatorCell is null || elevatorCell.Id != cellId)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotLocatedOppositeToTheSpecifiedCell", CommonUtils.Culture.Actual) };
            }

            // check #2: a loading unit must be on board of the elevator
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            if (loadingUnit is null
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("NoLoadingUnitIsOnBoardOfTheElevator", CommonUtils.Culture.Actual) };
            }

            // check #2: there is enough space to host the loading unit in the specified cell
            if (!this.cellsProvider.CanFitLoadingUnit(elevatorCell.Id, loadingUnit.Id))
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheLoadingUnitDoesNotFitInTheSpecifiedCell", CommonUtils.Culture.Actual) };
            }

            // check #3: elevator's pawl cannot be be in zero position
            if (this.machineResourcesProvider.IsSensorZeroOnCradle)
            {
                return new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual) };
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
                return new ActionPolicy { Reason = Resources.Cells.ResourceManager.GetString("TheSpecifiedCellIsNotWithinTheElevatorVerticalBounds", CommonUtils.Culture.Actual) };
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

            return new AxisBounds { Upper = verticalAxis.UpperBound, Lower = verticalAxis.LowerBound, Offset = verticalAxis.Offset };
        }

        public void Homing(Axis calibrateAxis, Calibration calibration, int? loadUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender)
        {
            IHomingMessageData homingData = new HomingMessageData(calibrateAxis, calibration, loadUnitId, showErrors, false);

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

            var supposedLoadingUnitGrossWeight = bayPosition.LoadingUnit.MaxNetWeight + bayPosition.LoadingUnit.Tare + this.elevatorDataProvider.GetWeight();

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
        /// <param name="waitContinue">true: the inverter positioning state machine stops after the transmission of parameters and waits for a Continue command before enabling inverter
        ///                             the scope is to wait for the shutter to open or close before moving </param>
        /// <param name="requestingBay">the requesting bay</param>
        /// <param name="sender">The sender of message</param>
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
            int? sourceBayPositionId = null,
            bool fastDeposit = true)
        {
            if (!this.machineVolatileDataProvider.IsOneTonMachine.Value)
            {
                // Perform the horizontal movement for regular machine (no combined movements)
                this.MoveHorizontalAuto_ForRegularMachine(
                    direction,
                    isLoadingUnitOnBoard,
                    loadingUnitId,
                    loadingUnitGrossWeight,
                    waitContinue,
                    measure,
                    requestingBay,
                    sender,
                    targetCellId,
                    targetBayPositionId,
                    sourceCellId,
                    sourceBayPositionId,
                    fastDeposit);
            }
            else
            {
                // Perform the horizontal movement for 1 Ton machine (with combined movements)
                this.MoveHorizontalAuto_ForOneTonMachine(
                    direction,
                    isLoadingUnitOnBoard,
                    loadingUnitId,
                    loadingUnitGrossWeight,
                    waitContinue,
                    measure,
                    requestingBay,
                    sender,
                    targetCellId,
                    targetBayPositionId,
                    sourceCellId,
                    sourceBayPositionId,
                    fastDeposit);
            }
        }

        public void MoveHorizontalCalibration(BayNumber requestingBay, MessageActor sender)
        {
            var bay = this.baysDataProvider.GetByNumber(requestingBay);
            var policy = this.CanCalibrateZeroPlate();
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var direction = (bay.Side == WarehouseSide.Back) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);
            if (axis.HorizontalCalibrateSpeed == 0)
            {
                policy = new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheHorizontalCalibrateSpeedCannotBeZero", CommonUtils.Culture.Actual) };
                throw new InvalidOperationException(policy.Reason);
            }

            var targetPosition = 100;

            targetPosition *= (direction == HorizontalMovementDirection.Forwards) ? 1 : -1;

            var speed = new[] { axis.HorizontalCalibrateSpeed };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Absolute,
                MovementMode.HorizontalCalibration,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Horizontal calibration Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);

            // close other bays
            var bays = this.baysDataProvider.GetAll();
            foreach (var otherBay in bays)
            {
                if (otherBay.Shutter != null
                    && otherBay.Shutter.Type != ShutterType.NotSpecified
                    && otherBay.Number != bay.Number
                    )
                {
                    var shutterInverter = otherBay.Shutter.Inverter.Index;
                    var shutterPosition = this.machineResourcesProvider.GetShutterPosition(shutterInverter);
                    if (shutterPosition != ShutterPosition.Closed)
                    {
                        this.shutterProvider.MoveTo(ShutterPosition.Closed, otherBay.Number, MessageActor.AutomationService);
                    }
                }
            }
        }

        public void MoveHorizontalFindZero(BayNumber requestingBay, MessageActor sender)
        {
            var policy = ActionPolicy.Allowed;

            if (this.machineResourcesProvider.IsSensorZeroOnCradle
                ||
                !this.machineResourcesProvider.IsDrawerCompletelyOffCradle)
            {
                policy = new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheElevatorIsNotEmptyButThePawlIsInZeroPosition", CommonUtils.Culture.Actual) };
                throw new InvalidOperationException(policy.Reason);
            }

            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            if (axis.HomingCreepSpeed == 0)
            {
                policy = new ActionPolicy { Reason = Resources.Elevator.ResourceManager.GetString("TheHomingCreepSpeedCannotBeZero", CommonUtils.Culture.Actual) };
                throw new InvalidOperationException(policy.Reason);
            }

            var targetPosition = this.HorizontalPosition + 400;

            var speed = new[] { axis.HomingCreepSpeed };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Absolute,
                MovementMode.FindZero,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Forwards);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} Horizontal find zero Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Positioning,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        /// <summary>
        /// Moves the horizontal chain of the elevator to load or unload a LoadUnit in manual mode.
        /// It does not use a selected profile to perform this operation.
        /// </summary>
        public void MoveHorizontalManual(HorizontalMovementDirection direction,
            double distance,
            double verticalDisplacement,
            bool measure,
            int? loadingUnitId,
            int? positionId,
            bool bypassConditions,
            BayNumber requestingBay,
            MessageActor sender,
            bool highSpeed)
        {
            if (!highSpeed || distance < 0)
            {
                this.machineVolatileDataProvider.IsHomingExecuted = false;
            }
            if (!this.machineVolatileDataProvider.IsOneTonMachine.Value)
            {
                // Perform the horizontal movement for regular machine (no combined movements)
                this.MoveHorizontalManual_ForRegularMachine(
                    direction,
                    distance,
                    measure,
                    loadingUnitId,
                    positionId,
                    bypassConditions,
                    requestingBay,
                    sender,
                    highSpeed);
            }
            else
            {
                // Perform the horizontal movement for 1 Ton machine (with combined movements)
                this.MoveHorizontalManual_ForOneTonMachine(
                    direction,
                    distance,
                    verticalDisplacement,
                    measure,
                    loadingUnitId,
                    positionId,
                    bypassConditions,
                    requestingBay,
                    sender,
                    highSpeed);
            }
        }

        public void MoveHorizontalProfileCalibration(int bayPositionId, BayNumber requestingBay, MessageActor sender)
        {
            var policy = this.CanLoadFromBay(bayPositionId, requestingBay, isGuided: false, enforceLoadUnitPresent: false);
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
                MovementType.Absolute,
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

        public void MoveHorizontalResolution(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender)
        {
            var bay = this.baysDataProvider.GetByNumber(requestingBay);
            var policy = this.CanCalibrateZeroPlate();
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }
            var verticalPosition = this.elevatorDataProvider.VerticalPosition;
            var otherBay = this.baysDataProvider.GetAll().FirstOrDefault(b => b.Number != requestingBay
                        && b.Side != bay.Side
                        && b.Positions.Any(p => Math.Abs(p.Height - verticalPosition) < 700));
            if (otherBay != null && otherBay.Shutter != null && otherBay.Shutter.Type != ShutterType.NotSpecified)
            {
                var shutterInverter = this.baysDataProvider.GetShutterInverterIndex(otherBay.Number);
                var position = this.sensorsProvider.GetShutterPosition(shutterInverter);
                if (position != ShutterPosition.Opened)
                {
                    throw new InvalidOperationException(string.Format(Resources.Shutters.ResourceManager.GetString("TheShutterOfBayIsNotCompletelyOpen", CommonUtils.Culture.Actual), (int)otherBay.Number));
                }
            }
            var axis = this.elevatorDataProvider.GetAxis(Orientation.Horizontal);

            var profileTypeLoad = this.SelectProfileType(direction, false);
            var profileTypeDeposit = this.SelectProfileType(direction, true);
            var targetPosition = axis.Profiles.Single(p => p.Name == profileTypeLoad).TotalDistance +
                axis.Profiles.Single(p => p.Name == profileTypeDeposit).TotalDistance;

            targetPosition *= (direction == HorizontalMovementDirection.Forwards) ? 1 : -1;

            var procedureParameters = this.setupProceduresDataProvider.GetHorizontalResolutionCalibration();

            var speed = new[] { axis.ManualMovements.FeedRateAfterZero * axis.FullLoadMovement.Speed };
            var acceleration = new[] { axis.FullLoadMovement.Acceleration };
            var deceleration = new[] { axis.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };

            var messageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Absolute,
                MovementMode.HorizontalResolution,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                procedureParameters.RequiredCycles,
                lowerBound: 0,
                upperBound: 0,
                delay: 0,
                switchPosition,
                direction);

            this.PublishCommand(
                messageData,
                $"Execute {Axis.Horizontal} resolution Command",
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
            int? loadUnitId,
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
                loadUnitId,
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
                loadUnitId: null,
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
                loadUnitId: null,
                targetBayPositionId: null,
                cellId,
                checkHomingDone: true,
                waitContinue: false);
        }

        public void MoveToFreeCell(int loadUnitId, bool computeElongation, bool performWeighting, BayNumber requestingBay, MessageActor sender)
        {
            var cellId = this.cellsProvider.FindEmptyCell(loadUnitId);
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
                loadUnitId,
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
                    Resources.Elevator.ResourceManager.GetString("MovementDistanceCannotBeZero", CommonUtils.Culture.Actual));
            }

            var homingDone = this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay];
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.ResourceManager.GetString("VerticalOriginCalibrationMustBePerformed", CommonUtils.Culture.Actual));
            }

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            var manualParameters = this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical);
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
            var homingDone = this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay];
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
                targetPosition = parameters.TargetDistance.Value * (direction == VerticalMovementDirection.Up ? 1 : -1);
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

            messageData.BypassConditions = true;

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

        public void ResetEnduranceTest()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetDepositAndPickUpTest();

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
                    Resources.Elevator.ResourceManager.GetString("MovementDistanceCannotBeZero", CommonUtils.Culture.Actual));
            }

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            var homingDone = this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay];
            if (!homingDone)
            {
                throw new InvalidOperationException(Resources.Elevator.ResourceManager.GetString("VerticalOriginCalibrationMustBePerformed", CommonUtils.Culture.Actual));
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
                    Resources.BeltBurnishingProcedure.ResourceManager.GetString("UpperBoundPositionMustBeStrictlyPositive", CommonUtils.Culture.Actual));
            }

            if (upperBoundPosition <= lowerBoundPosition)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerBoundPosition),
                    Resources.BeltBurnishingProcedure.ResourceManager.GetString("UpperBoundPositionMustBeStrictlyGreaterThanLowerBoundPosition", CommonUtils.Culture.Actual));
            }

            var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);

            if (upperBoundPosition > verticalAxis.UpperBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(upperBoundPosition),
                    Resources.BeltBurnishingProcedure.ResourceManager.GetString("UpperBoundPositionOutOfRange", CommonUtils.Culture.Actual));
            }

            if (lowerBoundPosition < verticalAxis.LowerBound)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lowerBoundPosition),
                    Resources.BeltBurnishingProcedure.ResourceManager.GetString("LowerBoundPositionOutOfRange", CommonUtils.Culture.Actual));
            }

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];

            var procedureParameters = this.setupProceduresDataProvider.GetBeltBurnishingTest();

            var homingDone = this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay];

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

        public void StartRepetitiveHorizontalMovements(int bayPositionId, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            var policy = this.CanLoadFromBay(bayPositionId, requestingBay, isGuided: false);
            if (!policy.IsAllowed)
            {
                throw new InvalidOperationException(policy.Reason);
            }

            // Retrieve the required cycles from procedure Parameters object
            var procedureParameters = this.setupProceduresDataProvider.GetDepositAndPickUpTest();

            var delayStart = 0;
            var data = new RepetitiveHorizontalMovementsMessageData(
                bayPositionId,
                loadingUnitId,
                requestingBay,
                procedureParameters.RequiredCycles,
                delayStart);

            this.PublishCommand(
                data,
                "Execute Repetitive Horizontal Movements Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.RepetitiveHorizontalMovements,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void Stop(BayNumber requestingBay, MessageActor sender)
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Elevator Command",
                MessageActor.DeviceManager,
                sender,
                MessageType.Stop,
                requestingBay,
                BayNumber.ElevatorBay);
        }

        public void StopTest(BayNumber requestingBay, MessageActor sender)
        {
            this.PublishCommand(
                null,
                $"Stop Test Command on bay {requestingBay}",
                MessageActor.DeviceManager,
                sender,
                MessageType.StopTest,
                requestingBay,
                BayNumber.ElevatorBay); // mandatory!!!!
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
            int? loadUnitId,
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
                throw new InvalidOperationException(string.Format(Resources.Elevator.ResourceManager.GetString("OffsetOutOfBounds", CommonUtils.Culture.Actual), verticalAxis.Offset, lowerBound, upperBound));
            }

            if (targetPosition < lowerBound)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.DestinationBelowLowerBound, requestingBay);
                throw new InvalidOperationException(string.Format(Resources.Elevator.ResourceManager.GetString("TargetPositionOutOfBounds", CommonUtils.Culture.Actual), targetPosition, lowerBound, upperBound));
            }
            if (targetPosition > upperBound)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.DestinationOverUpperBound, requestingBay);
                throw new InvalidOperationException(string.Format(Resources.Elevator.ResourceManager.GetString("TargetPositionOutOfBounds", CommonUtils.Culture.Actual), targetPosition, lowerBound, upperBound));
            }

            var homingDone = !checkHomingDone || this.machineVolatileDataProvider.IsBayHomingExecuted[BayNumber.ElevatorBay];

            if (!homingDone && !manualMovement)
            {
                throw new InvalidOperationException(string.Format(Resources.Elevator.ResourceManager.GetString("VerticalOriginCalibrationMustBePerformed", CommonUtils.Culture.Actual), verticalAxis.Offset, lowerBound, upperBound));
            }

            var sensors = this.sensorsProvider.GetAll();
            var isLoadingUnitOnBoard =
                sensors[(int)IOMachineSensors.LuPresentInMachineSide]
                &&
                sensors[(int)IOMachineSensors.LuPresentInOperatorSide];
            if (movementMode == MovementMode.PositionAndMeasureWeight
                && !isLoadingUnitOnBoard)
            {
                this.logger.LogWarning($"Do not measure weight on empty elevator!");
                movementMode = MovementMode.Position;
            }
            var measureDistance = verticalAxis.WeightMeasurement.MeasureSpeed * verticalAxis.WeightMeasurement.MeasureTime / 10 + 50;
            if (movementMode == MovementMode.PositionAndMeasureWeight
                && this.VerticalPosition > upperBound - measureDistance)
            {
                this.logger.LogWarning($"Not enough space to measure weight!");
                this.errorsProvider.RecordNew(MachineErrorCode.DestinationOverUpperBound, requestingBay);
                throw new InvalidOperationException(string.Format(Resources.Elevator.ResourceManager.GetString("TargetPositionOutOfBounds", CommonUtils.Culture.Actual), targetPosition, lowerBound, upperBound));
            }

            if (computeElongation && !isLoadingUnitOnBoard)
            {
                if (this.machineVolatileDataProvider.IsOneTonMachine.Value)
                {
                    this.logger.LogWarning($"Do not compute elongation on empty elevator in 1T machine!");
                }
                else if (loadUnitId.HasValue)
                {
                    var lu = this.loadingUnitsDataProvider.GetById(loadUnitId.Value);
                    var beltDisplacement = this.invertersProvider.ComputeDisplacement(targetPosition, lu.GrossWeight);
                    this.logger.LogInformation($"Vertical positioning with Belt elongation for height={targetPosition:0.00} and weight={lu.GrossWeight:0.00} is {beltDisplacement:0.00} [mm].");
                    targetPosition += beltDisplacement <= 6 ? beltDisplacement / 2 : 3; // displacement with empty elevator is limited to 3mm
                }
                else
                {
                    this.logger.LogWarning($"Do not compute elongation on empty elevator!");
                }
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
            if (this.machineVolatileDataProvider.Mode == MachineMode.FirstTest || this.machineVolatileDataProvider.Mode == MachineMode.FirstTest2 || this.machineVolatileDataProvider.Mode == MachineMode.FirstTest3)
            {
                // during first load unit test the feedrate is fixed to half speed
                feedRate /= 2;
            }
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
                $"targetPosition: {targetPosition:0.00}; " +
                $"homing: {homingDone}; " +
                $"feedRate: {(sender == MessageActor.AutomationService ? feedRate : 1)}; " +
                $"speed: {speed[0]:0.00}; " +
                $"acceleration: {acceleration[0]:0.00}; " +
                $"deceleration: {deceleration[0]:0.00}; " +
                $"speed w/o feedRate: {movementParameters.Speed:0.00}; " +
                $"Load Unit {messageData.LoadingUnitId.GetValueOrDefault()}");

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
