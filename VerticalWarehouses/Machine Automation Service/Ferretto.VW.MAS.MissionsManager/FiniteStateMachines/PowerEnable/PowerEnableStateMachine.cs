using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.PowerEnable.States;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.PowerEnable
{
    internal class PowerEnableStateMachine : FiniteStateMachine<IPowerEnableStartState>, IPowerEnableStateMachine
    {


        #region Constructors

        public PowerEnableStateMachine(
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        { }

        #endregion



        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Type == MessageType.PowerEnable;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return notification.Type == MessageType.PowerEnable;
        }

        protected override IState OnCommandReceived(CommandMessage command)
        {
            var newState = base.OnCommandReceived(command);
            if(newState != this.ActiveState)
            {
                return newState;
            }

            if(command.Data is IPowerEnableMessageData commandData)
            {
                switch(commandData.CommandAction)
                {
                    case CommandAction.Stop:
                    break;
                }
            }
            return newState;
        }

        protected override IState OnNotificationReceived(NotificationMessage notificationMessage)
        {
            var newState = base.OnNotificationReceived(notificationMessage);
            if(newState != this.ActiveState)
            {
                return newState;
            }

            newState = this.ActiveState.NotificationReceived(notificationMessage);
            if(newState != this.ActiveState)
            {
                return newState;
            }

            return newState;
        }

        #endregion
    }
}
