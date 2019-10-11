using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit
{
    internal class MoveLoadingUnitStateMachine : FiniteStateMachine<IMoveLoadingUnitStartSate>, IMoveLoadingUnitStateMachine
    {
        #region Constructors

        public MoveLoadingUnitStateMachine(
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
        }

        #endregion

        #region Methods

        public override bool AllowMultipleInstances(CommandMessage command)
        {
            return true;
        }

        public override void Start(CommandMessage commandMessage, IFiniteStateMachineData machineData, CancellationToken cancellationToken)
        {
            if (machineData is IMoveLoadingUnitMachineData)
            {
                base.Start(commandMessage, machineData, cancellationToken);
            }
            else
            {
                var description = $"Attempting to start {this.GetType()} Finite state machine with wrong ({machineData.GetType()}) machine data";

                throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
            }
        }

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Type == MessageType.MoveLoadingUnit;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            throw new NotImplementedException();
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
