using System;
using System.Collections.Generic;
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

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private BayNumber closeShutter;

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
            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();
            this.closeShutter = BayNumber.None;
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (commandMessage.Data is IMoveLoadingUnitMessageData messageData
                && machineData is Mission moveData
                )
            {
                this.Logger.LogDebug($"{this.GetType().Name}: {moveData}");
                this.mission = moveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitStartState);
                this.missionsDataProvider.Update(this.mission);

                if (moveData.LoadingUnitSource is LoadingUnitLocation.Elevator)
                {
                    var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(moveData, out var targetBayPositionId, out var targetCellId);
                    if (destinationHeight is null)
                    {
                        var description = $"GetSourceHeight error: position not found ({moveData.LoadingUnitSource} {(moveData.LoadingUnitSource == LoadingUnitLocation.Cell ? moveData.LoadingUnitCellSourceId : moveData.LoadingUnitId)})";

                        throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                    }
                    if (targetCellId != null)
                    {
                        var bay = this.loadingUnitMovementProvider.GetBayByCell(targetCellId.Value);
                        if (bay != BayNumber.None)
                        {
                            this.closeShutter = bay;
                        }
                    }

                    this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                        this.closeShutter,
                        measure: false,
                        MessageActor.MachineManager,
                        commandMessage.RequestingBay,
                        moveData.RestoreConditions,
                        targetBayPositionId,
                        targetCellId);
                }
                else
                {
                    var sourceHeight = this.loadingUnitMovementProvider.GetSourceHeight(moveData, out var targetBayPositionId, out var targetCellId);

                    if (sourceHeight is null)
                    {
                        var description = $"GetSourceHeight error: position not found ({moveData.LoadingUnitSource} {(moveData.LoadingUnitSource == LoadingUnitLocation.Cell ? moveData.LoadingUnitCellSourceId : moveData.LoadingUnitId)})";

                        throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                    }

                    this.loadingUnitMovementProvider.PositionElevatorToPosition(sourceHeight.Value,
                        this.closeShutter,
                        measure: false,
                        MessageActor.MachineManager,
                        commandMessage.RequestingBay,
                        moveData.RestoreConditions,
                        targetBayPositionId,
                        targetCellId);
                }
                bool isEject = this.mission.LoadingUnitDestination != LoadingUnitLocation.Cell
                    && this.mission.LoadingUnitDestination != LoadingUnitLocation.Elevator
                    && this.mission.LoadingUnitDestination != LoadingUnitLocation.LoadingUnit
                    && this.mission.LoadingUnitDestination != LoadingUnitLocation.NoLocation;

                var newMessageData = new MoveLoadingUnitMessageData(
                    this.mission.MissionType,
                    this.mission.LoadingUnitSource,
                    this.mission.LoadingUnitDestination,
                    this.mission.LoadingUnitCellSourceId,
                    this.mission.DestinationCellId,
                    this.mission.LoadingUnitId,
                    (this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell),
                    isEject,
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
                moveData.Status = MissionStatus.Executing;
                this.mission.RestoreConditions = false;
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
                    this.UpdateResponseList(notificationStatus, notification.Type);
                    if ((this.closeShutter != BayNumber.None && this.stateMachineResponses.Count == 2) || (this.closeShutter == BayNumber.None && this.stateMachineResponses.Count == 1))
                    {
                        if (this.mission.LoadingUnitSource is LoadingUnitLocation.Elevator)
                        {
                            returnValue = this.GetState<IMoveLoadingUnitDepositUnitState>();
                        }
                        else
                        {
                            returnValue = this.GetState<IMoveLoadingUnitLoadElevatorState>();
                        }
                    }
                    break;

                case MessageStatus.OperationStop:
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
            if (this.mission != null
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

        private void UpdateResponseList(MessageStatus status, MessageType messageType)
        {
            if (this.stateMachineResponses.TryGetValue(messageType, out var stateMachineResponse))
            {
                stateMachineResponse = status;
                this.stateMachineResponses[messageType] = stateMachineResponse;
            }
            else
            {
                this.stateMachineResponses.Add(messageType, status);
            }
        }

        #endregion
    }
}
