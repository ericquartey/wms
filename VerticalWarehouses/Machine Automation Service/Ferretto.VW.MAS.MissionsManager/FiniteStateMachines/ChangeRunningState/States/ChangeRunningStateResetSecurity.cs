using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States
{
    internal class ChangeRunningStateResetSecurity : StateBase, IChangeRunningStateResetSecurity
    {

        #region Fields

        private readonly IMachineControlProvider machineControlProvider;

        #endregion

        #region Constructors

        public ChangeRunningStateResetSecurity(
            IMachineControlProvider machineControlProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineControlProvider = machineControlProvider ??
                throw new ArgumentNullException(nameof(machineControlProvider));
        }

        #endregion



        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if (commandMessage.Data is IChangeRunningStateMessageData messageData)
            {
                if (messageData.Enable)
                {
                    this.machineControlProvider.ResetSecurity(MessageActor.MissionsManager, commandMessage.RequestingBay);
                }
                else
                {
                    // TODO Define a cleanup pattern for State Machine after this error
                    this.NotifyCommandError(commandMessage, "Power Enable Reset Fault State started during machine power down");
                }
            }
            else
            {
                // TODO Define a cleanup pattern for State Machine after this error
                this.NotifyCommandError(commandMessage, $"Power Enable Reset Fault State received wrong initialization data ({commandMessage.Data.GetType()})");
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.machineControlProvider.ResetSecurityStatus(notification);

            if (notificationStatus != MessageStatus.NoStatus)
            {
                switch (notificationStatus)
                {
                    case MessageStatus.OperationEnd:
                        returnValue = this.GetState<IChangeRunningStateEndState>();
                        returnValue.StopRequestReason = StopRequestReason.NoReason;
                        break;

                    case MessageStatus.OperationError:
                        returnValue = this.GetState<IChangeRunningStateEndState>();
                        returnValue.StopRequestReason = StopRequestReason.Error;
                        returnValue.ErrorMessage = notification;
                        break;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
