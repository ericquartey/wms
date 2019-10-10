using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
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
                    var description = "Power Enable Reset Fault State started during machine power down";

                    throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
                }
            }
            else
            {
                var description = $"Power Enable Reset Fault State received wrong initialization data ({commandMessage.Data.GetType()})";

                throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
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
                        returnValue = this.GetState<IChangeRunningStateInverterPowerSwitch>();
                        break;

                    case MessageStatus.OperationError:
                        returnValue = this.GetState<IChangeRunningStateEndState>();

                        ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                        ((IEndState)returnValue).ErrorMessage = notification;
                        break;
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

        #endregion
    }
}
