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
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateStartState : StateBase, IChangeRunningStateStartState
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        private bool requestedState;

        #endregion

        #region Constructors

        public ChangeRunningStateStartState(
            IBaysProvider baysProvider,
            IMachineControlProvider machineControlProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
            : base(eventAggregator, logger)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));

            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                if (messageData.Enable)
                {
                    this.requestedState = true;

                    this.machineControlProvider.StartChangePowerStatus(messageData, MessageActor.MissionsManager, commandMessage.RequestingBay);
                }
                else
                {
                    this.requestedState = false;

                    var newMessageData = new StopMessageData(messageData.StopReason);
                    this.machineControlProvider.StopOperation(newMessageData, BayNumber.All, MessageActor.MissionsManager, commandMessage.RequestingBay);
                }
            }
            else
            {
                var description = $"Power Enable Start State received wrong initialization data ({commandMessage.Data.GetType()})";

                throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.PowerStatusChangeStatus(notification);

            if (notificationStatus != MessageStatus.NoStatus)
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
                if (notificationStatus != MessageStatus.NoStatus)
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
                        this.machineControlProvider.StartChangePowerStatus(messageData, MessageActor.MissionsManager, notification.RequestingBay);
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
