using System;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
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

        private bool isMovingForward;

        private bool isMovingManual;

        private bool isMovingShutter;

        private Mission mission;

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
            this.Logger.LogDebug($"MoveLoadingUnitErrorState: received command {commandMessage.Type}, {commandMessage.Description}");

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
                this.isMovingManual = false;
                this.isMovingShutter = false;
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            if (this.isMovingManual
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
                            if (shutterPosition == ShutterPosition.Opened
                                || shutterPosition == ShutterPosition.NotSpecified)
                            {
                                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
                                {
                                    this.isMovingManual = true;
                                    this.isMovingShutter = false;
                                }
                                else
                                {
                                    returnValue = this.RestoreOriginalStep();
                                }
                            }
                            else
                            {
                                this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitShutterClosed);
                                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                                this.isMovingShutter = false;
                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.mission.TargetBay);
                            }
                        }
                        else
                        {
                            this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal positioning end");
                            if (this.isMovingManual)
                            {
                                returnValue = this.RestoreOriginalStep();
                            }
                            else
                            {
                                this.isMovingForward = false;
                                this.loadingUnitMovementProvider.UpdateLastIdealPosition(this.direction, true);
                                returnValue = this.DepositEnd();
                            }
                        }
                        break;

                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationRunningStop:
                        {
                            this.isMovingManual = false;
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

            switch (this.mission.FsmRestoreStateName)
            {
                case nameof(MoveLoadingUnitBayChainState):
                    returnValue = this.MoveLoadUnitBayChain();
                    break;

                case nameof(MoveLoadingUnitCloseShutterState):
                    this.mission.RestoreConditions = true;
                    this.mission.FsmRestoreStateName = null;
                    returnValue = this.GetState<IMoveLoadingUnitCloseShutterState>();
                    break;

                case nameof(MoveLoadingUnitDepositUnitState):
                    returnValue = this.MoveLoadUnitDeposit();
                    break;

                case nameof(MoveLoadingUnitEndState):
                    this.mission.FsmRestoreStateName = null;
                    this.mission.RestoreConditions = false;
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                    break;

                case nameof(MoveLoadingUnitLoadElevatorState):
                    returnValue = this.MoveLoadUnitLoadElevatorBack();
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

        private IState DepositEnd()
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

        private IState MoveLoadUnitBayChain()
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

        /// <summary>
        /// we try to resolve different error situations:
        /// 1)  if the elevator has moved vertically we can start from previous step: if elevator is not full will be checked later
        /// 2)  if we have to deposit in cell we try to move chain forward in manual mode calculating the distance. Then repeat deposit step
        /// 3)  if we have to deposit in bay we have more cases:
        ///     3.1) shutter is open: try to move forward in manual mode calculating the distance
        ///     3.2) shutter is closed or intermediate: open the shutter and move chain back. Then repeat deposit step
        /// </summary>
        /// <returns></returns>
        private IState MoveLoadUnitDeposit()
        {
            IState returnValue = this;
            var destination = this.loadingUnitMovementProvider.GetDestinationHeight(this.mission);
            var current = this.loadingUnitMovementProvider.GetCurrentVerticalPosition();
            if (!destination.HasValue || Math.Abs((destination.Value - current)) > 2)
            {
                // the conservative approach
                //this.Logger.LogError($"MoveLoadingUnitErrorState: Vertical position is not valid {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                //returnValue = this.GetState<IMoveLoadingUnitEndState>();
                //((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;

                // the brave approach
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Vertical position has changed {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                this.mission.RestoreConditions = true;
                returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();

                return returnValue;
            }

            this.direction = HorizontalMovementDirection.Backwards;
            var toBay = false;
            var openShutter = false;
            switch (this.mission.LoadingUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.mission.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.mission.DestinationCellId.Value);
                        // original direction
                        this.direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                    // original direction
                    this.direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    openShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                    toBay = true;
                    break;
            }
            if (!toBay)
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal forward positioning start");
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, true, MessageActor.MachineManager, this.mission.TargetBay))
                {
                    this.isMovingForward = true;
                }
                else
                {
                    returnValue = this.RestoreOriginalStep();
                }
            }
            else
            {
                var shutterPosition = this.sensorsProvider.GetShutterPosition(this.mission.TargetBay);
                if (openShutter
                    && shutterPosition != ShutterPosition.Opened
                    && shutterPosition != ShutterPosition.NotSpecified
                    )
                {
                    this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Shutter positioning start");
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.mission.TargetBay, true);
                    this.isMovingShutter = true;
                    // invert direction
                    this.direction = (this.direction == HorizontalMovementDirection.Backwards) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                }
                else
                {
                    this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal forward positioning start");
                    if (this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, true, MessageActor.MachineManager, this.mission.TargetBay))
                    {
                        this.isMovingForward = true;
                    }
                    else
                    {
                        returnValue = this.RestoreOriginalStep();
                    }
                }
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
        private IState MoveLoadUnitLoadElevatorBack()
        {
            IState returnValue = this;
            var origin = this.loadingUnitMovementProvider.GetSourceHeight(this.mission);
            var current = this.loadingUnitMovementProvider.GetCurrentVerticalPosition();
            if (!origin.HasValue || Math.Abs((origin.Value - current)) > 2)
            {
                // the conservative approach
                //this.Logger.LogError($"MoveLoadingUnitErrorState: Vertical position is not valid {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                //returnValue = this.GetState<IMoveLoadingUnitEndState>();
                //((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;

                // the brave approach
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Vertical position has changed {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

                this.mission.RestoreConditions = true;
                returnValue = this.GetState<IMoveLoadingUnitStartState>();

                return returnValue;
            }

            this.direction = HorizontalMovementDirection.Backwards;
            var measure = false;
            var openShutter = false;
            switch (this.mission.LoadingUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.mission.LoadingUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.mission.LoadingUnitCellSourceId.Value);

                        // invert direction
                        this.direction = cell.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitSource);
                    // invert direction
                    this.direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    openShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                    measure = true;
                    break;
            }
            if (!measure)
            {
                this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal back positioning start");
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
                {
                    this.isMovingManual = true;
                }
                else
                {
                    returnValue = this.RestoreOriginalStep();
                }
            }
            else
            {
                var shutterPosition = this.sensorsProvider.GetShutterPosition(this.mission.TargetBay);
                if (openShutter
                    && shutterPosition != ShutterPosition.Opened
                    && shutterPosition != ShutterPosition.NotSpecified
                    )
                {
                    this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Shutter positioning start");
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.mission.TargetBay, true);
                    this.isMovingShutter = true;
                }
                else
                {
                    this.Logger.LogDebug($"MoveLoadingUnitErrorState: Manual Horizontal back positioning start");
                    if (this.loadingUnitMovementProvider.MoveManualLoadingUnitBack(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
                    {
                        this.isMovingManual = true;
                    }
                    else
                    {
                        returnValue = this.RestoreOriginalStep();
                    }
                }
            }
            return returnValue;
        }

        private IState RestoreOriginalStep()
        {
            IState returnValue;
            this.isMovingManual = false;
            this.isMovingForward = false;
            this.isMovingShutter = false;
            this.mission.RestoreConditions = true;
            if (this.mission.FsmRestoreStateName == nameof(MoveLoadingUnitLoadElevatorState))
            {
                returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
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
