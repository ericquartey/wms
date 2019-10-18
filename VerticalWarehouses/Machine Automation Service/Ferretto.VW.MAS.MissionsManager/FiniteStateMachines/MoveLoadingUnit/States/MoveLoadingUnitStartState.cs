using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
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

        private List<MovementMode> movements;

        private bool needOpenShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitStartState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger)
            : base(eventAggregator, logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.movements = new List<MovementMode>();
            this.needOpenShutter = false;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData)
            {
                var sourceHeight = this.loadingUnitMovementProvider.GetSourceHeight(messageData);

                if (sourceHeight == 0)
                {
                    var description = $"GetSourceHeight error: position not found ({messageData.Source} {(messageData.Source == LoadingUnitLocation.Cell ? messageData.SourceCellId : messageData.LoadingUnitId)})";

                    throw new StateMachineException(description, commandMessage, MessageActor.MissionsManager);
                }

                this.movements = this.loadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight, messageData.Source, MessageActor.MissionsManager, commandMessage.RequestingBay);
                this.needOpenShutter = this.loadingUnitMovementProvider.NeedOpenShutter(messageData.Source);
            }
            else
            {
                var description = $"Move Loading Unit Start State received wrong initialization data ({commandMessage.Data.GetType().Name})";

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
                        returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
                    }
                    break;

                case MessageStatus.OperationError:
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

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
