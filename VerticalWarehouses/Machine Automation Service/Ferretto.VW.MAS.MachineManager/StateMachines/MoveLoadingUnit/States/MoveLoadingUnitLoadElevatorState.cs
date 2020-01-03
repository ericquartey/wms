using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.FiniteStateMachines.MoveLoadingUnit.States
{
    internal class MoveLoadingUnitLoadElevatorState : StateBase, IMoveLoadingUnitLoadElevatorState, IProgressMessageState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitMovementProvider loadingUnitMovementProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private HorizontalMovementDirection direction;

        private bool measure;

        private Mission mission;

        private ShutterPosition openShutter;

        private Dictionary<MessageType, MessageStatus> stateMachineResponses;

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
            this.measure = false;
            this.direction = HorizontalMovementDirection.Backwards;
        }

        #endregion

        #region Properties

        public NotificationMessage Message { get; set; }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            if (machineData is Mission moveData)
            {
                this.Logger.LogDebug($"{this.GetType().Name}: {moveData}");
                this.mission = moveData;
                this.mission.FsmStateName = nameof(MoveLoadingUnitLoadElevatorState);
                this.missionsDataProvider.Update(this.mission);

                this.direction = HorizontalMovementDirection.Backwards;
                this.measure = false;
                switch (this.mission.LoadingUnitSource)
                {
                    case LoadingUnitLocation.Cell:
                        if (this.mission.LoadingUnitCellSourceId != null)
                        {
                            var cell = this.cellsProvider.GetById(this.mission.LoadingUnitCellSourceId.Value);

                            this.direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }

                        break;

                    default:
                        var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.mission.LoadingUnitSource);
                        if (bay is null)
                        {
                            var description = $"{this.GetType().Name}: source bay not found {moveData.LoadingUnitSource}";

                            throw new StateMachineException(description, commandMessage, MessageActor.MachineManager);
                        }
                        this.direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        this.openShutter = this.loadingUnitMovementProvider.GetShutterOpenPosition(bay, this.mission.LoadingUnitSource);
                        if (this.openShutter == this.sensorsProvider.GetShutterPosition(bay.Number))
                        {
                            this.openShutter = ShutterPosition.NotSpecified;
                        }
                        this.measure = true;
                        if (bay.Carousel != null)
                        {
                            var result = this.loadingUnitMovementProvider.CheckBaySensors(bay, moveData.LoadingUnitSource, deposit: false);
                            if (result != MachineErrorCode.NoError)
                            {
                                var error = this.errorsProvider.RecordNew(result);
                                throw new StateMachineException(error.Description, commandMessage, MessageActor.MachineManager);
                            }
                        }
                        break;
                }
                if (this.mission.NeedHomingAxis == Axis.Horizontal)
                {
                    this.Logger.LogDebug($"Homing elevator occupied start");
                    this.loadingUnitMovementProvider.Homing(Axis.HorizontalAndVertical, Calibration.FindSensor, this.mission.LoadingUnitId, commandMessage.RequestingBay, MessageActor.MachineManager);
                }
                else if (this.mission.NeedHomingAxis == Axis.BayChain)
                {
                    if (this.openShutter != ShutterPosition.NotSpecified)
                    {
                        this.Logger.LogDebug($"OpenShutter start");
                        this.loadingUnitMovementProvider.OpenShutter(MessageActor.MachineManager, this.openShutter, this.mission.TargetBay, false);
                    }
                    else
                    {
                        this.Logger.LogDebug($"MoveManualLoadingUnitForward start: direction {this.direction}");
                        this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, false, this.measure, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
                    }
                }
                else
                {
                    this.Logger.LogDebug($"MoveLoadingUnit start: direction {this.direction}, openShutter {this.openShutter}, measure {this.measure}");
                    this.loadingUnitMovementProvider.MoveLoadingUnit(this.direction, true, this.openShutter, this.measure, MessageActor.MachineManager, this.mission.TargetBay, moveData.LoadingUnitId);
                }
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
                    if (notification.Type == MessageType.Homing)
                    {
                        if (this.mission.NeedHomingAxis == Axis.Horizontal)
                        {
                            this.Logger.LogDebug($"MoveLoadingUnit start: direction {this.direction}, openShutter {this.openShutter}, measure {this.measure}");
                            this.loadingUnitMovementProvider.MoveLoadingUnit(this.direction, true, this.openShutter, this.measure, MessageActor.MachineManager, notification.RequestingBay, this.mission.LoadingUnitId);
                        }
                        else if (this.mission.NeedHomingAxis == Axis.BayChain)
                        {
                            returnValue = this.LoadUnitEnd();
                        }
                        this.mission.NeedHomingAxis = Axis.None;
                        this.missionsDataProvider.Update(this.mission);
                    }
                    else
                    {
                        this.UpdateResponseList(notificationStatus, notification.Type);

                        if (notification.Type == MessageType.ShutterPositioning)
                        {
                            var shutterPosition = this.sensorsProvider.GetShutterPosition(notification.RequestingBay);
                            if (shutterPosition == this.openShutter)
                            {
                                if (this.mission.NeedHomingAxis == Axis.BayChain)
                                {
                                    this.Logger.LogDebug($"MoveManualLoadingUnitForward start: direction {this.direction}");
                                    this.loadingUnitMovementProvider.MoveManualLoadingUnitForward(this.direction, false, this.measure, this.mission.LoadingUnitId, MessageActor.MachineManager, this.mission.TargetBay);
                                }
                                else
                                {
                                    this.Logger.LogDebug($"ContinuePositioning");
                                    this.loadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                                }
                            }
                            else
                            {
                                this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                                returnValue = this.OnStop(StopRequestReason.Error);
                                if (returnValue is IEndState endState)
                                {
                                    endState.ErrorMessage = notification;
                                }
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
                this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();
                if (this.mission.NeedHomingAxis == Axis.BayChain)
                {
                    this.Logger.LogDebug($"Homing Bay free start");
                    this.loadingUnitMovementProvider.Homing(Axis.BayChain, Calibration.FindSensor, this.mission.LoadingUnitId, notification.RequestingBay, MessageActor.MachineManager);
                }
                else
                {
                    returnValue = this.LoadUnitEnd();
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
                if (!this.errorsProvider.IsErrorSmall())
                {
                    this.mission.NeedMovingBackward = true;
                }
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

        private IState LoadUnitEnd()
        {
            IState returnValue;
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
            this.Message = new NotificationMessage(
                            null,
                            $"Load Unit position changed",
                            MessageActor.Any,
                            MessageActor.MachineManager,
                            MessageType.Positioning,
                            this.mission.TargetBay,
                            this.mission.TargetBay,
                            MessageStatus.OperationUpdateData);

            // in bay-to-cell movements the profile may have changed so we have to find a new empty cell
            if (this.mission.LoadingUnitSource != LoadingUnitLocation.Cell
                && this.mission.LoadingUnitDestination == LoadingUnitLocation.Cell
                && this.mission.LoadingUnitId > 0
                )
            {
                try
                {
                    this.mission.DestinationCellId = this.cellsProvider.FindEmptyCell(this.mission.LoadingUnitId);
                }
                catch (InvalidOperationException)
                {
                    // cell not found: go back to bay
                    this.errorsProvider.RecordNew(MachineErrorCode.WarehouseIsFull);
                    this.mission.LoadingUnitDestination = this.mission.LoadingUnitSource;
                    return this.GetState<IMoveLoadingUnitDepositUnitState>();
                }
            }

            if (this.mission.LoadingUnitSource == LoadingUnitLocation.Cell
                && this.mission.LoadingUnitDestination == LoadingUnitLocation.Elevator
                )
            {
                returnValue = this.GetState<IMoveLoadingUnitEndState>();
            }
            else
            {
                returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();
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
