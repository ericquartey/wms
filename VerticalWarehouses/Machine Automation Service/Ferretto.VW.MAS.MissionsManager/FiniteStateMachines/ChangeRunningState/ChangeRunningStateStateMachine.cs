using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Providers.Interfaces;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
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
            return this.machineControlProvider.FilterCommands(command);
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return this.machineControlProvider.FilterNotifications(notification);
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
            if (newState is IChangeRunningStateEndState endState)
            {
                var newMessage = new NotificationMessage(
                    this.StartData.Data,
                    this.StartData.Description,
                    MessageActor.AutomationService,
                    MessageActor.MissionsManager,
                    this.StartData.Type,
                    this.StartData.RequestingBay,
                    this.StartData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(endState.StopRequestReason));

                var eventArgs = new FiniteStateMachinesEventArgs { InstanceId = this.InstanceId, NotificationMessage = newMessage };
                this.RaiseCompleted(eventArgs);
            }

            if (newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        #endregion
    }
}
