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

        private readonly IBaysProvider baysProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private IMoveLoadingUnitMessageData messageData;

        private IMoveLoadingUnitMachineData moveData;

        private bool openShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitDepositUnitState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IElevatorDataProvider elevatorDataProvider,
            IBaysProvider baysProvider,
            ICellsProvider cellsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));

            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();
            this.openShutter = false;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData && machineData is IMoveLoadingUnitMachineData moveData)
            {
                this.messageData = messageData;
                this.moveData = moveData;

                var direction = HorizontalMovementDirection.Backwards;
                switch (this.messageData.Destination)
                {
                    case LoadingUnitLocation.Cell:
                        if (this.messageData.DestinationCellId != null)
                        {
                            var cell = this.cellsProvider.GetById(this.messageData.DestinationCellId.Value);

                            direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        }

                        break;

                    default:
                        var bay = this.baysProvider.GetByLoadingUnitLocation(this.messageData.Destination);
                        direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
                        this.openShutter = true;

                        break;
                }

                this.loadingUnitMovementProvider.MoveLoadingUnit(direction, false, this.openShutter, MessageActor.MachineManager, commandMessage.RequestingBay, null);
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
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                    ((IEndState)returnValue).ErrorMessage = notification;
                    break;
            }

            if ((this.openShutter && this.stateMachineResponses.Count == 2) || (!this.openShutter && this.stateMachineResponses.Count == 1))
            {
                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                {
                    this.elevatorDataProvider.SetLoadingUnit(null);

                    if (this.messageData.Destination is LoadingUnitLocation.Cell)
                    {
                        var destinationCellId = this.messageData.DestinationCellId;
                        if (destinationCellId.HasValue)
                        {
                            this.cellsProvider.SetLoadingUnit(destinationCellId.Value, this.moveData.LoadingUnitId);
                        }
                        else
                        {
                            throw new InvalidOperationException("Loading unit movement to target cell has no target cell specified.");
                        }
                    }
                    else
                    {
                        var bayPosition = this.baysProvider.GetPositionByLocation(this.messageData.Destination);
                        this.baysProvider.SetLoadingUnit(bayPosition.Id, this.moveData.LoadingUnitId);
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
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

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
