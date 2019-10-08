using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState
{
    internal class ChangeRunningStateStateMachine : FiniteStateMachine<IChangeRunningStateStartState>, IChangeRunningStateStateMachine
    {
        #region Fields

        private readonly IMachineControlProvider machineControlProvider;

        #endregion

        #region Constructors

        public ChangeRunningStateStateMachine(
            IMachineControlProvider machineControlProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineControlProvider = machineControlProvider ?? throw new ArgumentNullException(nameof(machineControlProvider));
        }

        #endregion

        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Type == MessageType.ChangeRunningState;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return this.machineControlProvider.FilterNotifications(notification, MessageActor.MissionsManager);
        }

        protected override IState OnCommandReceived(CommandMessage commandMessage)
        {
            var newState = base.OnCommandReceived(commandMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.CommandReceived(commandMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        protected override IState OnNotificationReceived(NotificationMessage notificationMessage)
        {
            var newState = base.OnNotificationReceived(notificationMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.NotificationReceived(notificationMessage);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        #endregion
    }
}
