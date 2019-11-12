// ReSharper disable ArrangeThisQualifier

using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateStartState : StateBase, IChangeRunningStateStartState, IStartMessageState
    {
        #region Fields

        private readonly IBaysDataProvider baysProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private bool requestedState;

        #endregion

        #region Constructors

        public ChangeRunningStateStartState(
            IBaysDataProvider baysProvider,
            IMachineControlProvider machineControlProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));

            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                if (messageData.Enable)
                {
                    this.requestedState = true;

                    this.machineControlProvider.StartChangePowerStatus(messageData, MessageActor.MachineManager, commandMessage.RequestingBay);
                }
                else
                {
                    this.requestedState = false;

                    var newMessageData = new StopMessageData(messageData.StopReason);
                    this.machineControlProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MachineManager, commandMessage.RequestingBay);
                }

                var notificationData = new ChangeRunningStateMessageData(
                    messageData.Enable,
                    machineData.MachineId,
                    messageData.CommandAction,
                    messageData.StopReason,
                    messageData.Verbosity);

                this.Message = new NotificationMessage(
                    notificationData,
                    $"Started Change Running State to {messageData.Enable}",
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.ChangeRunningState,
                    commandMessage.RequestingBay,
                    commandMessage.TargetBay,
                    MessageStatus.OperationStart);
            }
            else
            {
                var description = $"Power Enable Start State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.PowerStatusChangeStatus(notification);

            if (notificationStatus != MessageStatus.NotSpecified)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        returnValue = this.requestedState ? this.GetState<IChangeRunningStateResetFaultState>() : this.GetState<IChangeRunningStateInverterPowerSwitch>();
                        break;

                    case MessageStatus.OperationError:
                        returnValue = this.GetState<IChangeRunningStateEndState>();

                        ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                        ((IEndState)returnValue).ErrorMessage = notification;
                        break;
                }
            }
            else
            {
                notificationStatus = this.machineControlProvider.StopOperationStatus(notification);
                if (notificationStatus != MessageStatus.NotSpecified)
                {
                    switch (notificationStatus)
                    {
                        case MessageStatus.OperationEnd:
                            this.UpdateResponseList(notificationStatus, notification.TargetBay);
                            break;

                        case MessageStatus.OperationError:
                            returnValue = this.GetState<IChangeRunningStateEndState>();

                            ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                            ((IEndState)returnValue).ErrorMessage = notification;
                            break;
                    }

                    if (this.stateMachineResponses.Values.Count == this.baysProvider.GetAll().Count())
                    {
                        var messageData = new ChangeRunningStateMessageData(this.requestedState);
                        this.machineControlProvider.StartChangePowerStatus(messageData, MessageActor.MachineManager, notification.RequestingBay);
                    }
                }
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IChangeRunningStateEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

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
