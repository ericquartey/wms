using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.ChangeRunningState.States;
using Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitStartSate : StateBase, IMoveLoadingUnitStartSate
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        #endregion

        #region Constructors

        public MoveLoadingUnitStartSate(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                this.loadingUnitMovementProvider.PositionElevatorToSource(messageData, MessageActor.MissionsManager, commandMessage.RequestingBay);
            }
            else
            {
                var description = $"Move Loading Unit Start Sate received wrong initialization data ({commandMessage.Data.GetType()})";

                throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToSourceStatus(notification);

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
