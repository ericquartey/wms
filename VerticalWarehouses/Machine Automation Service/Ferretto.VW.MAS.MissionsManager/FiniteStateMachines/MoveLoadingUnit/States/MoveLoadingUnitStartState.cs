using System;
using System.Collections.Generic;
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

// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitStartState : StateBase, IMoveLoadingUnitStartState
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly List<MovementMode> movements;

        private bool needOpenShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitStartState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.movements = new List<MovementMode>();
            this.needOpenShutter = false;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData && machineData is MoveLoadingUnitMachineData moveData)
            {
                var sourceHeight = this.loadingUnitMovementProvider.GetSourceHeight(messageData, out var loadingUnitId);

                if (sourceHeight == 0)
                {
                    var description = $"GetSourceHeight error: position not found ({messageData.Source})";

                    throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
                }

                moveData.LoadingUnitId = loadingUnitId;

                this.loadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight, LoadingUnitDestination.NoDestination, MessageActor.MissionsManager, commandMessage.RequestingBay);
                this.needOpenShutter = this.loadingUnitMovementProvider.NeedOpenShutter(messageData.Source);
            }
            else
            {
                var description = $"Move Loading Unit Start State received wrong initialization data ({commandMessage.Data.GetType()})";

                throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification, this.movements);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    if (this.needOpenShutter)
                    {
                        returnValue = this.GetState<IMoveLoadingUnitOpenShutterState>();
                    }
                    else
                    {
                        returnValue = this.GetState<IMoveLoadingUnitLoadUnitState>();
                    }
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
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

            return returnValue;
        }

        #endregion
    }
}
