using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitStartState : StateBase, IMoveLoadingUnitStartState, IStartMessageState
    {
        #region Fields

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitStartState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData
                && machineData is Mission moveData
                )
            {
                this.mission = moveData;
                var sourceHeight = this.loadingUnitMovementProvider.GetSourceHeight(moveData);

                if (sourceHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({moveData.LoadingUnitSource} {(moveData.LoadingUnitSource == LoadingUnitLocation.Cell ? moveData.LoadingUnitCellSourceId : moveData.LoadingUnitId)})";

                    throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                }

                this.loadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value, false, false, MessageActor.MachineManager, commandMessage.RequestingBay);

                var newMessageData = new MoveLoadingUnitMessageData(
                    messageData.MissionType,
                    messageData.Source,
                    messageData.Destination,
                    messageData.SourceCellId,
                    messageData.DestinationCellId,
                    messageData.LoadingUnitId,
                    messageData.InsertLoadingUnit,
                    messageData.EjectLoadingUnit,
                    moveData.FsmId,
                    messageData.CommandAction,
                    messageData.StopReason,
                    messageData.Verbosity);

                this.Message = new NotificationMessage(
                    newMessageData,
                    $"Loading Unit {moveData.LoadingUnitId} start movement to bay {messageData.Destination}",
                    MessageActor.AutomationService,
                    MessageActor.MachineManager,
                    MessageType.MoveLoadingUnit,
                    commandMessage.RequestingBay,
                    commandMessage.TargetBay,
                    MessageStatus.OperationStart);
                moveData.FsmStateName = this.GetType().Name;
                moveData.Status = MissionStatus.Executing;
                this.missionsDataProvider.Update(moveData);
            }
            else
            {
                var description = $"Move Loading Unit Start State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.PositionElevatorToPositionStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.OnStop(StopRequestReason.Error);
                    if (returnValue is IEndState endState)
                    {
                        endState.ErrorMessage = notification;
                    }
                    break;
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            IState returnValue;
            if (reason == StopRequestReason.Error
                && this.mission.IsRestoringType()
                )
            {
                this.mission.FsmRestoreStateName = this.mission.FsmStateName;
                returnValue = this.GetState<IMoveLoadingUnitErrorState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            if (returnValue is IEndState endState)
            {
                endState.StopRequestReason = reason;
            }

            return returnValue;
        }

        #endregion
    }
}
