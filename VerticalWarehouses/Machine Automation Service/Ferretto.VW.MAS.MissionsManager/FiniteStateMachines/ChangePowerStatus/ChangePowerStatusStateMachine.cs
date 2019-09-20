using System.Collections.Generic;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangePowerStatus.States;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangePowerStatus
{
    internal class ChangePowerStatusStateMachine : FiniteStateMachine<IChangePowerStatusStartState>, IChangePowerStatusStateMachine
    {

        #region Fields

        private readonly List<Bay> configuredBays;

        #endregion

        #region Constructors

        public ChangePowerStatusStateMachine(
            BayNumber requestingBay,
            List<Bay> configuredBays,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(requestingBay, eventAggregator, logger, serviceScopeFactory)
        {

            this.configuredBays = configuredBays;
        }

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
            if (newState != this.ActiveState)
            {
                return newState;
            }

            if (command.Data is IPowerEnableMessageData commandData)
            {
                switch (commandData.CommandAction)
                {
                    case CommandAction.Stop:
                        break;
                }
            }
            return newState;
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            var newState = base.OnNotificationReceived(notification);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            switch (this.ActiveState)
            {
                case IChangePowerStatusStartState _:
                    {
                        break;
                    }
            }

            return newState;
        }

        #endregion
    }
}
