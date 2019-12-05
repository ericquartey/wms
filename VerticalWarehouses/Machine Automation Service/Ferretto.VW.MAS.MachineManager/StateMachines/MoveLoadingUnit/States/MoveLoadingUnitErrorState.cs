using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitErrorState : StateBase, IMoveLoadingUnitErrorState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private HorizontalMovementDirection direction;

        private bool isMovingManual;

        private bool isMovingShutter;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitErrorState(
            IBaysDataProvider baysDataProvider,
            IMissionsDataProvider missionsDataProvider,
            ICellsProvider cellsProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMachineProvider machineProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");

            if (machineData is Mission moveData)
            {
                this.mission = moveData;
            }

            var newMessageData = new StopMessageData(StopRequestReason.Error);
            this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
            this.mission.RestoreConditions = false;
            this.mission.FsmStateName = this.GetType().Name;
            this.missionsDataProvider.Update(this.mission);
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.StopOperationStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    // State machine is in error, any response from device manager state machines will do to complete state machine shutdown
                    case MessageStatus.OperationError:
                    case MessageStatus.OperationEnd:
                    case MessageStatus.OperationStop:
                    case MessageStatus.OperationRunningStop:
                        this.UpdateResponseList(notificationStatus, notification.TargetBay);
                        break;
                }
            }

            if (this.isMovingManual || this.isMovingShutter)
            {
                notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterPosition = this.sensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == ShutterPosition.Opened
                                || shutterPosition == ShutterPosition.NotSpecified)
                            {
                                if (this.loadingUnitMovementProvider.MoveManualLoadingUnit(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
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

                                this.isMovingManual = false;
                                this.isMovingShutter = false;
                                this.stateMachineResponses.Clear();
                                var newMessageData = new StopMessageData(StopRequestReason.Error);
                                this.loadingUnitMovementProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, this.mission.TargetBay);
                            }
                        }
                        else
                        {
                            returnValue = this.RestoreOriginalStep();
                        }
                        break;

                    case MessageStatus.OperationError:
                    case MessageStatus.OperationRunningStop:
                        {
                            this.isMovingManual = false;
                            this.isMovingShutter = false;
                            this.stateMachineResponses.Clear();
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

            this.Logger.LogDebug($"{this.GetType().Name}: Resume mission {this.mission.Id}, wmsId {this.mission.WmsId}, from {this.mission.FsmRestoreStateName}, loadUnit {this.mission.LoadingUnitId}");

            switch (this.mission.FsmRestoreStateName)
            {
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
                    this.Logger.LogError($"{this.GetType().Name}: no valid FsmRestoreStateName {this.mission.FsmRestoreStateName} for mission {this.mission.Id}, wmsId {this.mission.WmsId}, loadUnit {this.mission.LoadingUnitId}");

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

        private IState MoveLoadUnitDeposit()
        {
            IState returnValue = this;
            this.direction = HorizontalMovementDirection.Backwards;
            var measure = false;
            var openShutter = false;
            switch (this.mission.LoadingUnitDestination)
            {
                case LoadingUnitLocation.Cell:
                    if (this.mission.DestinationCellId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.mission.DestinationCellId.Value);

                        this.direction = cell.Side != WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitDestination);
                    this.direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                    openShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                    measure = true;
                    break;
            }
            if (!measure)
            {
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnit(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
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
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.mission.TargetBay, true);
                    this.isMovingShutter = true;
                }
                else
                {
                    if (this.loadingUnitMovementProvider.MoveManualLoadingUnit(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
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

        private IState MoveLoadUnitLoadElevatorBack()
        {
            IState returnValue = this;
            this.direction = HorizontalMovementDirection.Backwards;
            var measure = false;
            var openShutter = false;
            switch (this.mission.LoadingUnitSource)
            {
                case LoadingUnitLocation.Cell:
                    if (this.mission.LoadingUnitCellSourceId != null)
                    {
                        var cell = this.cellsProvider.GetById(this.mission.LoadingUnitCellSourceId.Value);

                        this.direction = cell.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    }

                    break;

                default:
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitSource);
                    this.direction = bay.Side != WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                    openShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                    measure = true;
                    break;
            }
            if (!measure)
            {
                if (this.loadingUnitMovementProvider.MoveManualLoadingUnit(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
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
                    this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.mission.TargetBay, true);
                    this.isMovingShutter = true;
                }
                else
                {
                    if (this.loadingUnitMovementProvider.MoveManualLoadingUnit(this.direction, MessageActor.MachineManager, this.mission.TargetBay))
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

        private void UpdateResponseList(MessageStatus status, BayNumber targetBay)
        {
            if (this.stateMachineResponses.TryGetValue(targetBay, out var stateMachineResponse))
            {
                stateMachineResponse = status;
                this.stateMachineResponses[targetBay] = stateMachineResponse;
            }
            else
            {
                this.stateMachineResponses.Add(targetBay, status);
            }
        }

        #endregion
    }
}
