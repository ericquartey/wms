using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitMoveToTargetState : StateBase, IMoveLoadingUnitMoveToTargetState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private BayNumber closeShutter;

        private bool ejectLoadUnit;

        private bool measure;

        private Mission mission;

        #endregion

        #region Constructors

        public MoveLoadingUnitMoveToTargetState(
            IBaysDataProvider baysDataProvider,
            IErrorsProvider errorsProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));

            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();

            this.closeShutter = BayNumber.None;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.measure = false;
            this.ejectLoadUnit = false;
            if (machineData is Mission moveData)
            {
                this.Logger.LogDebug($"{this.GetType().Name}: {moveData}");
                this.mission = moveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitMoveToTargetState);
                this.missionsDataProvider.Update(this.mission);

                if (moveData.LoadingUnitSource != LoadingUnitLocation.Cell)
                {
                    var bay = this.baysDataProvider.GetByLoadingUnitLocation(moveData.LoadingUnitSource);
                    if (bay is null)
                    {
                        var description = $"{this.GetType().Name}: source bay not found {moveData.LoadingUnitSource}";

                        throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                    }
                    this.closeShutter = (bay.Shutter.Type != ShutterType.NotSpecified ? bay.Number : BayNumber.None);
                    this.measure = true;
                }

                var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(moveData, out var targetBayPositionId, out var targetCellId);
                if (destinationHeight is null)
                {
                    var description = $"GetSourceHeight error: position not found ({moveData.LoadingUnitSource} {(moveData.LoadingUnitSource == LoadingUnitLocation.Cell ? moveData.LoadingUnitCellSourceId : moveData.LoadingUnitId)})";

                    throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                }
                if (this.mission.NeedHomingAxis == Axis.Horizontal)
                {
                    this.Logger.LogDebug($"Homing elevator occupied start");
                    this.loadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.mission.LoadingUnitId, commandMessage.RequestingBay, MessageActor.MachineManager);
                }
                else
                {
                    this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                        this.closeShutter,
                        this.measure,
                        MessageActor.MachineManager,
                        commandMessage.RequestingBay,
                        moveData.RestoreConditions,
                        targetBayPositionId,
                        targetCellId);
                }
                this.mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.mission);
            }
            else
            {
                var description = $"Move Loading Unit Move To Target State received wrong initialization data ({commandMessage.Data.GetType().Name})";

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
                    if (notification.Type == MessageType.Homing)
                    {
                        // do not clear needHoming because will have to do it after unloading (DepositUnitState)
                        var destinationHeight = this.loadingUnitMovementProvider.GetDestinationHeight(this.mission, out var targetBayPositionId, out var targetCellId);
                        this.loadingUnitMovementProvider.PositionElevatorToPosition(destinationHeight.Value,
                            this.closeShutter,
                            this.measure,
                            MessageActor.MachineManager,
                            notification.RequestingBay,
                            this.mission.RestoreConditions,
                            targetBayPositionId,
                            targetCellId);
                    }
                    else
                    {
                        if (!this.ejectLoadUnit
                            || notification.Type == MessageType.ShutterPositioning
                            )
                        {
                            this.UpdateResponseList(notificationStatus, notification.Type);
                        }
                    }

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationStop:
                case MessageStatus.OperationRunningStop:
                    if (this.ejectLoadUnit)
                    {
                        this.UpdateResponseList(notificationStatus, notification.Type);
                    }
                    else
                    {
                        returnValue = this.OnStop(StopRequestReason.Error);
                        if (returnValue is IEndState endState)
                        {
                            endState.ErrorMessage = notification;
                        }
                    }
                    break;

                case MessageStatus.OperationUpdateData:
                    // check weight value
                    if (this.measure
                        && !this.ejectLoadUnit
                        && notification.Source != MessageActor.MachineManager
                        )
                    {
                        var check = this.loadingUnitsDataProvider.CheckWeight(this.mission.LoadingUnitId);
                        if (check == MachineErrorCode.NoError)
                        {
                            this.baysDataProvider.Light(this.mission.TargetBay, false);
                        }
                        else
                        {
                            // stop movement and  go back to bay
                            this.errorsProvider.RecordNew(check);
                            this.ejectLoadUnit = true;
                            this.mission.LoadingUnitDestination = this.mission.LoadingUnitSource;
                            this.missionsDataProvider.Update(this.mission);
                            var newMessageData = new StopMessageData(StopRequestReason.Stop);
                            this.loadingUnitMovementProvider.StopOperation(newMessageData, notification.RequestingBay, MessageActor.MachineManager, notification.RequestingBay);
                        }
                    }

                    break;
            }

            if ((this.closeShutter != BayNumber.None && this.stateMachineResponses.Count == 2) || (this.closeShutter == BayNumber.None && this.stateMachineResponses.Count == 1))
            {
                if (this.mission.LoadingUnitDestination == LoadingUnitLocation.Elevator
                    && !this.ejectLoadUnit
                    )
                {
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                }
                else
                {
                    returnValue = this.GetState<IMoveLoadingUnitDepositUnitState>();
                }
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
