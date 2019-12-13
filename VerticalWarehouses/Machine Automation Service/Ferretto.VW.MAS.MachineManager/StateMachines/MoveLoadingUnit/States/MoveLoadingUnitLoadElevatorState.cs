using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitLoadElevatorState : StateBase, IMoveLoadingUnitLoadElevatorState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private Mission mission;

        private ShutterPosition openShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitLoadElevatorState(
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            IElevatorDataProvider elevatorDataProvider,
            IErrorsProvider errorsProvider,
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IMissionsDataProvider missionsDataProvider,
            ISensorsProvider sensorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));

            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();
            this.openShutter = ShutterPosition.NotSpecified;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"MoveLoadingUnitLoadElevatorState: received command {commandMessage.Type}, {commandMessage.Description}");

            if (machineData is Mission machineMoveData)
            {
                this.mission = machineMoveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitLoadElevatorState);
                this.missionsDataProvider.Update(this.mission);

                var direction = HorizontalMovementDirection.Backwards;
                bool measure = false;
                switch (this.mission.LoadingUnitSource)
                {
                    case LoadingUnitLocation.Cell:
                        if (this.mission.LoadingUnitCellSourceId != null)
                        {
                            var cell = this.cellsProvider.GetById(this.mission.LoadingUnitCellSourceId.Value);

                            direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }

                        break;

                    default:
                        var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitSource);
                        direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        this.openShutter = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.mission.LoadingUnitSource);
                        measure = true;
                        break;
                }

                this.loadingUnitMovementProvider.MoveLoadingUnit(direction, true, this.openShutter, measure, MessageActor.MachineManager, commandMessage.RequestingBay, machineMoveData.LoadingUnitId);
                this.mission.RestoreConditions = false;
                this.missionsDataProvider.Update(this.mission);
            }
            else
            {
                var description = $"Move Loading Unit Load Unit State received wrong initialization data ({commandMessage.Data.GetType().Name})";

                throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
            }
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            IState returnValue = this;

            var notificationStatus = this.loadingUnitMovementProvider.MoveLoadingUnitStatus(notification);

            switch (notificationStatus)
            {
                case MessageStatus.OperationEnd:
                    this.UpdateResponseList(notificationStatus, notification.Type);

                    if (notification.Type == MessageType.ShutterPositioning)
                    {
                        var shutterPosition = this.sensorsProvider.GetShutterPosition(notification.RequestingBay);
                        if (shutterPosition == this.openShutter)
                        {
                            this.loadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                        }
                        else
                        {
                            this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitShutterClosed);
                            this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                            returnValue = this.OnStop(StopRequestReason.Error);
                            if (returnValue is IEndState endState)
                            {
                                endState.ErrorMessage = notification;
                            }
                        }
                    }

                    break;

                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    {
                        returnValue = this.OnStop(StopRequestReason.Error);
                        if (returnValue is IEndState endState)
                        {
                            endState.ErrorMessage = notification;
                        }
                    }
                    break;
            }

            if ((this.openShutter != ShutterPosition.NotSpecified && this.stateMachineResponses.Count == 2)
                || (this.openShutter == ShutterPosition.NotSpecified && this.stateMachineResponses.Count == 1)
                )
            {
                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                {
                    this.elevatorDataProvider.SetLoadingUnit(this.mission.LoadingUnitId);

                    if (this.mission.LoadingUnitSource == LoadingUnitLocation.Cell)
                    {
                        var sourceCellId = this.mission.LoadingUnitCellSourceId;
                        if (sourceCellId.HasValue)
                        {
                            this.cellsProvider.SetLoadingUnit(sourceCellId.Value, null);
                        }
                        else
                        {
                            throw new InvalidOperationException("");
                        }
                    }
                    else
                    {
                        var bayPosition = this.baysDataProvider.GetPositionByLocation(this.mission.LoadingUnitSource);
                        this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                    }

                    transaction.Commit();
                }
                // in bay-to-cell movements the profile may have changed so we have to find a new empty cell
                if (this.mission.LoadingUnitSource != LoadingUnitLocation.Cell
                    && this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell
                    && this.mission.LoadingUnitId > 0
                    )
                {
                    this.mission.DestinationCellId = this.cellsProvider.FindEmptyCell(this.mission.LoadingUnitId);
                }
                if (this.mission.LoadingUnitDestination == LoadingUnitLocation.Elevator)
                {
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();
                }
                else
                {
                    returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();
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
