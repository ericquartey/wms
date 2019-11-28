using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitMoveToTargetState : StateBase, IMoveLoadingUnitMoveToTargetState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private bool closeShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitMoveToTargetState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IBaysDataProvider baysDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));

            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();

            this.closeShutter = false;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");
            bool measure = false;
            if (machineData is IMoveLoadingUnitMachineData moveData)
            {
                if (moveData.LoadingUnitSource != LoadingUnitLocation.Cell)
                {
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(moveData.LoadingUnitSource);
                    this.closeShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                    measure = true;
                }
                moveData.FsmStateName = this.GetType().Name;
            }

            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(messageData);
                if (destinationHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({messageData.Source} {(messageData.Source == LoadingUnitLocation.Cell ? messageData.SourceCellId : messageData.LoadingUnitId)})";

                    throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                }

                this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value, this.closeShutter, measure, MessageActor.MachineManager, commandMessage.RequestingBay);
            }
            else
            {
                var description = $"Move Loading Unit Move To Target State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    this.UpdateResponseList(notificationStatus, notification.Type);

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                    ((IEndState)returnValue).ErrorMessage = notification;
                    break;
            }

            if ((this.closeShutter && this.stateMachineResponses.Count == 2) || (!this.closeShutter && this.stateMachineResponses.Count == 1))
            {
                returnValue = this.GetState<IMoveLoadingUnitDepositUnitState>();
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
