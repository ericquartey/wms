using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitDepositUnitState : StateBase, IMoveLoadingUnitDepositUnitState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private Mission mission;

        private bool openShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitDepositUnitState(
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            IElevatorDataProvider elevatorDataProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));

            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();
            this.openShutter = false;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData
                && machineData is Mission moveData
                )
            {
                this.mission = moveData;

                var direction = HorizontalMovementDirection.Backwards;
                var bayNumber = commandMessage.RequestingBay;
                switch (messageData.Destination)
                {
                    case LoadingUnitLocation.Cell:
                        if (moveData.DestinationCellId != null)
                        {
                            var cell = this.cellsProvider.GetById(moveData.DestinationCellId.Value);

                            direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }

                        break;

                    default:
                        var bay = this.baysDataProvider.GetByLoadingUnitLocation(moveData.LoadingUnitDestination);
                        direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        this.openShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                        bayNumber = bay.Number;
                        break;
                }

                this.loadingUnitMovementProvider.MoveLoadingUnit(direction, false, this.openShutter, false, MessageActor.MachineManager, bayNumber, null);
                this.mission.FsmStateName = this.GetType().Name;
                this.missionsDataProvider.Update(this.mission);
            }
            else
            {
                var description = $"Move Loading Unit Deposit Unit Sate received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    this.UpdateResponseList(notificationStatus, notification.Type);

                    if (notification.Type == MessageType.ShutterPositioning)
                    {
                        this.loadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                    }

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.OnStop(StopRequestReason.Error);
                    if (returnValue is IEndState endState)
                    {
                        endState.ErrorMessage = notification;
                    }
                    break;
            }

            if ((this.openShutter && this.stateMachineResponses.Count == 2) || (!this.openShutter && this.stateMachineResponses.Count == 1))
            {
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
                    }

                    transaction.Commit();
                }

                if (this.openShutter)
                {
                    returnValue = this.GetState<IMoveLoadingUnitCloseShutterState>();
                }
                else
                {
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                }
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            IState returnValue;
            if (reason == StopRequestReason.Error
                && this.mission.IsRestoringType()
                )
            {
                this.mission.FsmRestoreStateName = this.mission.FsmStateName;
                returnValue = this.GetState<IMoveLoadingUnitErrorState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            if (returnValue is IEndState endState)
            {
                endState.StopRequestReason = reason;
            }

            return returnValue;
        }

        private void UpdateResponseList(MessageStatus status, MessageType messageType)
        {
            if (this.stateMachineResponses.TryGetValue(messageType, out var stateMachineResponse))
            {
                stateMachineResponse = status;
                this.stateMachineResponses[messageType] = stateMachineResponse;
            }
            else
            {
                this.stateMachineResponses.Add(messageType, status);
            }
        }

        #endregion
    }
}
