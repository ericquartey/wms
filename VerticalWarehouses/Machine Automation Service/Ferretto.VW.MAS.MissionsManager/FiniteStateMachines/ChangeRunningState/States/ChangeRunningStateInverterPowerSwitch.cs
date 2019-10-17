using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateInverterPowerSwitch : StateBase, IChangeRunningStateInverterPowerSwitch
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IMachineControlProvider machineControlProvider;

        private readonly Dictionary<BayNumber, MessageStatus> stateMachineResponses;

        #endregion

        #region Constructors

        public ChangeRunningStateInverterPowerSwitch(
            IBaysProvider baysProvider,
            IMachineControlProvider machineControlProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));

            this.machineControlProvider = machineControlProvider ??
                throw new ArgumentNullException(nameof(machineControlProvider));

            this.stateMachineResponses = new Dictionary<BayNumber, MessageStatus>();
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                var inverterMessageData = new InverterPowerEnableMessageData(messageData.Enable);
                this.machineControlProvider.StartInverterPowerChange(inverterMessageData, BayNumber.All, MessageActor.MissionsManager, commandMessage.RequestingBay);
            }
            else
            {
                // TODO Define a cleanup pattern for State Machine after this error
                this.NotifyCommandError(commandMessage, $"Power Enable Reset Fault State received wrong initialization data ({commandMessage.Data.GetType().Name})");
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.InverterPowerChangeStatus(notification);
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
                    returnValue = this.GetState<IChangeRunningStateEndState>();

                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.NoReason;
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
