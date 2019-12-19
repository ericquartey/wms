using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitErrorState : StateBase, IMoveLoadingUnitErrorState, IProgressMessageState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private HorizontalMovementDirection direction;

        private bool isMovingBackward;

        private bool isMovingForward;

        private bool isMovingShutter;

        private bool measure;

        private Mission mission;

        private ShutterPosition openShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitErrorState(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMissionsDataProvider missionsDataProvider,
            ICellsProvider cellsProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");

            if (machineData is Mission moveData)
            {
                this.mission = moveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitErrorState);
                this.missionsDataProvider.Update(this.mission);

                var newMessageData = new StopMessageData(StopRequestReason.Error);
                this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
                this.mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.mission);

                this.isMovingForward = false;
                this.isMovingBackward = false;
                this.isMovingShutter = false;
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            if (this.isMovingBackward
                || this.isMovingForward
                || this.isMovingShutter)
            {
                var notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Shutter positioning end");
                            var shutterPosition = this.sensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == this.openShutter)
                            {
                                this.isMovingShutter = false;
                                this.isMovingBackward = false;
                                this.isMovingForward = false;
                                if (this.mission.FsmRestoreStateName != nameof(MoveLoadingUnitCloseShutterState))
                                {
                                    if (this.mission.NeedMovingBackward)
                                    {
                                        this.isMovingBackward = this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.direction, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
                                    }
                                    else
                                    {
                                        var isLoaded = (this.mission.FsmRestoreStateName == nameof(MoveLoadingUnitDepositUnitState));
                                        this.isMovingForward = this.isMovingForward = this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, isLoaded, this.measure, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
                                    }
                                }
                                if (!this.isMovingForward && !this.isMovingBackward)
                                {
                                    returnValue = this.RestoreOriginalStep();
                                }
                            }
                            else
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                                this.isMovingShutter = false;
                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.mission.TargetBay);
                            }
                        }
                        else
                        {
                            this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal positioning end");
                            if (this.isMovingBackward)
                            {
                                this.mission.NeedMovingBackward = false;
                                if (this.mission.NeedHomingAxis == Axis.Horizontal
                                    && this.mission.FsmRestoreStateName == nameof(MoveLoadingUnitDepositUnitState)
                                    )
                                {
                                    returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();
                                }
                                else
                                {
                                    returnValue = this.RestoreOriginalStep();
                                }
                            }
                            else
                            {
                                this.isMovingForward = false;
                                this.loadingUnitMovementProvider.UpdateLastIdealPosition(this.direction, true);
                                if (this.mission.FsmRestoreStateName == nameof(MoveLoadingUnitLoadElevatorState))
                                {
                                    returnValue = this.RestoreLoadElevatorEnd();
                                }
                                else
                                {
                                    returnValue = this.RestoreDepositEnd();
                                }
                            }
                        }
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationRunningStop:
                        {
                            this.isMovingBackward = false;
                            this.isMovingForward = false;
                            this.isMovingShutter = false;
                            var newMessageData = new StopMessageData(StopRequestReason.Error);
                            this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.mission.TargetBay);
                        }
                        break;
                }
            }
            return returnValue;
        }

        protected override IState OnResume(CommandMessage commandMessage)
        {
            IState returnValue = this;

            this.Logger.LogDebug($"MoveLoadingUnitErrorState: Resume mission {this.mission.Id}, wmsId {this.mission.WmsId}, from {this.mission.FsmRestoreStateName}, loadUnit {this.mission.LoadingUnitId}");
            this.measure = false;

            switch (this.mission.FsmRestoreStateName)
            {
                case nameof(MoveLoadingUnitBayChainState):
                    returnValue = this.RestoreBayChain();
                    break;

                case nameof(MoveLoadingUnitCloseShutterState):
                    returnValue = this.RestoreCloseShutter();
                    break;

                case nameof(MoveLoadingUnitDepositUnitState):
                    returnValue = this.RestoreDepositStart();
                    break;

                case nameof(MoveLoadingUnitEndState):
                    this.mission.FsmRestoreStateName = null;
                    this.mission.RestoreConditions = false;
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                    break;

                case nameof(MoveLoadingUnitLoadElevatorState):
                    returnValue = this.RestoreLoadElevatorStart();
                    break;

                case nameof(MoveLoadingUnitMoveToTargetState):
                    this.mission.RestoreConditions = true;
                    this.mission.FsmRestoreStateName = null;
                    returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();
                    break;

                case nameof(MoveLoadingUnitStartState):
                    this.mission.FsmRestoreStateName = null;
                    this.mission.RestoreConditions = false;
                    returnValue = this.GetState<IMoveLoadingUnitStartState>();
                    break;

                case nameof(MoveLoadingUnitWaitEjectConfirm):
                    this.mission.FsmRestoreStateName = null;
                    this.mission.RestoreConditions = false;
                    returnValue = this.GetState<IMoveLoadingUnitWaitEjectConfirm>();
                    break;

                case nameof(MoveLoadingUnitWaitPickConfirm):
                    this.mission.FsmRestoreStateName = null;
                    this.mission.RestoreConditions = false;
                    returnValue = this.GetState<IMoveLoadingUnitWaitPickConfirm>();
                    break;

                default:
                    this.Logger.LogError($"MoveLoadingUnitErrorState: no valid FsmRestoreStateName {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
                    break;
            }
            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        private IState RestoreBayChain()
        {
            IState returnValue = this;

            if (this.loadingUnitMovementProvider.IsOnlyTopPositionOccupied(this.mission.TargetBay))
            {
                // movement is finished
                var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                var destination = bay.Positions.FirstOrDefault(p => p.IsUpper);
                if (destination is null)
                {
                    throw new StateMachineException($"Upper position not defined for bay {bay.Number}", null, MessageActor.MachineManager);
                }
                this.mission.LoadingUnitDestination = destination.Location;

                var origin = bay.Positions.FirstOrDefault(p => !p.IsUpper);
                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                {
                    this.baysDataProvider.SetLoadingUnit(destination.Id, this.mission.LoadingUnitId);
                    this.baysDataProvider.SetLoadingUnit(origin.Id, null);
                    transaction.Commit();
                }

                var newMessageData = new MoveLoadingUnitMessageData(
                    this.mission.MissionType,
                    this.mission.LoadingUnitSource,
                    this.mission.LoadingUnitDestination,
                    this.mission.LoadingUnitCellSourceId,
                    this.mission.DestinationCellId,
                    this.mission.LoadingUnitId,
                    (this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                    false,
                    this.mission.FsmId);

                this.Message = new NotificationMessage(
                    newMessageData,
                    $"Loading Unit {this.mission.LoadingUnitId} placed on bay {bay.Number}",
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.MoveLoadingUnit,
                    this.mission.TargetBay,
                    bay.Number,
                    MessageStatus.OperationWaitResume);

                this.mission.FsmRestoreStateName = null;
                this.mission.RestoreConditions = false;
                if (this.mission.WmsId.HasValue)
                {
                    returnValue = this.GetState<IMoveLoadingUnitWaitPickConfirm>();
                }
                else
                {
                    returnValue = this.GetState<IMoveLoadingUnitWaitEjectConfirm>();
                }
            }
            else
            {
                this.mission.RestoreConditions = true;
                this.mission.FsmRestoreStateName = null;
                returnValue = this.GetState<IMoveLoadingUnitBayChainState>();
            }

            return returnValue;
        }

        private IState RestoreCloseShutter()
        {
            IState returnValue = this;
            var shutterPosition = this.sensorsProvider.GetShutterPosition(this.mission.TargetBay);
            if (shutterPosition != ShutterPosition.Opened)
            {
                this.openShutter = ShutterPosition.Opened;
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Shutter positioning start");
                this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.openShutter, this.mission.TargetBay, true);
                this.isMovingShutter = true;
            }
            else
            {
                this.mission.RestoreConditions = true;
                this.mission.FsmRestoreStateName = null;
                returnValue = this.GetState<IMoveLoadingUnitCloseShutterState>();
            }
            return returnValue;
        }

        private IState RestoreDepositEnd()
        {
            IState returnValue;
            bool bayShutter = false;
            using (var transaction = this.elevatorDataProvider.GetContextTransaction())
            {
                this.elevatorDataProvider.SetLoadingUnit(null);

                if (this.mission.LoadingUnitDestination is LoadingUnitLocation.Cell)
                {
                    var destinationCellId = this.mission.DestinationCellId;
                    if (destinationCellId.HasValue)
                    {
                        this.cellsProvider.SetLoadingUnit(destinationCellId.Value, this.mission.LoadingUnitId);
                    }
                    else
                    {
                        throw new InvalidOperationException("Loading unit movement to target cell has no target cell specified.");
                    }
                }
                else
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.mission.LoadingUnitDestination);
                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, this.mission.LoadingUnitId);
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                    bayShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                }

                transaction.Commit();
            }

            if (bayShutter)
            {
                returnValue = this.GetState<IMoveLoadingUnitCloseShutterState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            return returnValue;
        }

        /// <summary>
        /// we try to resolve different error situations:
        /// 1)  if the elevator has moved vertically we can start from previous step: if elevator is not full will be checked later
        /// 2)  if we have to deposit in cell we try to move chain forward in manual mode calculating the distance. Then repeat deposit step
        /// 3)  if we have to deposit in bay we have more cases:
        ///     3.1) shutter is open: try to move forward in manual mode calculating the distance
        ///     3.2) shutter is closed or intermediate: open the shutter and move chain back. Then repeat deposit step
        /// </summary>
        /// <returns></returns>
        private IState RestoreDepositStart()
        {
            IState returnValue = this;
            var destination = this.loadingUnitMovementProvider.GetDestinationHeight(this.mission, out var targetBayPositionId, out var targetCellId);
            var current = this.loadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((!destination.HasValue || Math.Abs((destination.Value - current)) > 2)
                && this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator)
                )
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Vertical position has changed {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                this.mission.RestoreConditions = true;
                returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();

                return returnValue;
            }

            this.direction = HorizontalMovementDirection.Backwards;
            this.openShutter = ShutterPosition.NotSpecified;
            this.measure = false;
            switch (this.mission.LoadingUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.mission.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.mission.DestinationCellId.Value);
                        // invert direction?
                        if (this.mission.NeedMovingBackward)
                        {
                            this.direction = (cell.Side != WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }
                        else
                        {
                            this.direction = (cell.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                    // invert direction?
                    if (this.mission.NeedMovingBackward)
                    {
                        this.direction = (bay.Side != WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }
                    else
                    {
                        this.direction = (bay.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }
                    this.openShutter = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.mission.LoadingUnitDestination);
                    var shutterPosition = this.sensorsProvider.GetShutterPosition(this.mission.TargetBay);
                    if (shutterPosition != this.openShutter)
                    {
                        this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Shutter positioning start");
                        this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.openShutter, this.mission.TargetBay, true);
                        this.isMovingShutter = true;
                        return returnValue;
                    }
                    break;
            }
            this.isMovingBackward = false;
            this.isMovingForward = false;
            if (this.mission.NeedMovingBackward
                || this.mission.NeedHomingAxis == Axis.Horizontal
                )
            {
                this.mission.NeedMovingBackward = true;
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal back positioning start");
                this.isMovingBackward = this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.direction, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
            }
            else
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal forward positioning start");
                this.isMovingForward = this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, true, false, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
            }

            if (!this.isMovingBackward && !this.isMovingForward)
            {
                returnValue = this.RestoreOriginalStep();
            }
            return returnValue;
        }

        private IState RestoreLoadElevatorEnd()
        {
            IState returnValue;
            using (var transaction = this.elevatorDataProvider.GetContextTransaction())
            {
                this.elevatorDataProvider.SetLoadingUnit(this.mission.LoadingUnitId);

                if (this.mission.LoadingUnitSource == LoadingUnitLocation.Cell)
                {
                    var sourceCellId = this.mission.LoadingUnitCellSourceId;
                    if (sourceCellId.HasValue)
                    {
                        this.cellsProvider.SetLoadingUnit(sourceCellId.Value, null);
                    }
                    else
                    {
                        throw new InvalidOperationException("");
                    }
                }
                else
                {
                    var bayPosition = this.baysDataProvider.GetPositionByLocation(this.mission.LoadingUnitSource);
                    this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                }

                transaction.Commit();
            }

            // in bay-to-cell movements the profile may have changed so we have to find a new empty cell
            if (this.mission.LoadingUnitSource != LoadingUnitLocation.Cell
                && this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell
                && this.mission.LoadingUnitId > 0
                )
            {
                this.mission.DestinationCellId = this.cellsProvider.FindEmptyCell(this.mission.LoadingUnitId);
            }

            if (this.mission.LoadingUnitDestination == LoadingUnitLocation.Elevator)
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();
            }
            return returnValue;
        }

        /// <summary>
        /// we try to resolve different error situations:
        /// 1)  if the elevator has moved vertically we can start from previous step: if elevator is not full will be checked later
        /// 2)  if we have to load from cell we move chain back in manual mode (it is safer to have elevator empty). Then repeat loading step
        /// 3)  if we have to load from bay we have more cases:
        ///     3.1) shutter is open: try to move chain back in manual mode (profile measure is not possible in manual mode). Then repeat loading step
        ///     3.2) shutter is closed or intermediate: open the shutter and start from 3.1)
        /// </summary>
        /// <returns></returns>
        private IState RestoreLoadElevatorStart()
        {
            IState returnValue = this;
            var origin = this.loadingUnitMovementProvider.GetSourceHeight(this.mission, out var targetBayPositionId, out var targetCellId);
            var current = this.loadingUnitMovementProvider.GetCurrentVerticalPosition();
            if ((!origin.HasValue || Math.Abs((origin.Value - current)) > 2)
                && !this.sensorsProvider.IsLoadingUnitInLocation(LoadingUnitLocation.Elevator)
                )
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Vertical position has changed {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                this.mission.RestoreConditions = true;
                returnValue = this.GetState<IMoveLoadingUnitStartState>();

                return returnValue;
            }

            this.direction = HorizontalMovementDirection.Backwards;
            this.measure = false;
            this.openShutter = ShutterPosition.NotSpecified;
            switch (this.mission.LoadingUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.mission.LoadingUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.mission.LoadingUnitCellSourceId.Value);

                        // invert direction?
                        if (this.mission.NeedMovingBackward)
                        {
                            this.direction = cell.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }
                        else
                        {
                            this.direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitSource);
                    // invert direction?
                    if (this.mission.NeedMovingBackward)
                    {
                        this.direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    else
                    {
                        this.direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }
                    this.openShutter = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.mission.LoadingUnitSource);
                    this.measure = true;
                    var shutterPosition = this.sensorsProvider.GetShutterPosition(this.mission.TargetBay);
                    if (shutterPosition != this.openShutter)
                    {
                        this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Shutter positioning start");
                        this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.openShutter, this.mission.TargetBay, true);
                        this.isMovingShutter = true;
                        return returnValue;
                    }
                    break;
            }
            this.isMovingBackward = false;
            this.isMovingForward = false;
            if (this.mission.NeedMovingBackward)
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal back positioning start");
                this.isMovingBackward = this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.direction, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
            }
            else
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal forward positioning start");
                this.isMovingForward = this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, false, this.measure, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
            }

            if (!this.isMovingBackward && !this.isMovingForward)
            {
                returnValue = this.RestoreOriginalStep();
            }
            return returnValue;
        }

        private IState RestoreOriginalStep()
        {
            IState returnValue;
            this.isMovingBackward = false;
            this.isMovingForward = false;
            this.isMovingShutter = false;
            this.mission.RestoreConditions = true;
            if (this.mission.FsmRestoreStateName == nameof(MoveLoadingUnitLoadElevatorState))
            {
                returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
            }
            else if (this.mission.FsmRestoreStateName == nameof(MoveLoadingUnitCloseShutterState))
            {
                returnValue = this.GetState<IMoveLoadingUnitCloseShutterState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitDepositUnitState>();
            }
            this.mission.FsmRestoreStateName = null;

            return returnValue;
        }

        #endregion
    }
}
