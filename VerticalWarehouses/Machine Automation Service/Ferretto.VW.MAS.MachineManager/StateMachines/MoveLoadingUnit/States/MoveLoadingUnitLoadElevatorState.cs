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

        private readonly ISensorsProvider sensorsProvider;

        private readonly Dictionary<MessageType, MessageStatus> stateMachineResponses;

        private IMoveLoadingUnitMachineData moveData;

        private bool openShutter;

        #endregion

        #region Constructors

        public MoveLoadingUnitLoadElevatorState(
            ILoadingUnitMovementProvider loadingUnitMovementProvider,
            IElevatorDataProvider elevatorDataProvider,
            IBaysDataProvider baysDataProvider,
            ICellsProvider cellsProvider,
            ISensorsProvider sensorsProvider,
            IErrorsProvider errorsProvider,
            ILogger<StateBase> logger)
            : base(logger)
        {
            this.loadingUnitMovementProvider = loadingUnitMovementProvider ?? throw new ArgumentNullException(nameof(loadingUnitMovementProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));

            this.stateMachineResponses = new Dictionary<MessageType, MessageStatus>();
            this.openShutter = false;
        }

        #endregion

        #region Methods

        protected override void OnEnter(CommandMessage commandMessage, IFiniteStateMachineData machineData)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: received command {commandMessage.Type}, {commandMessage.Description}");
            if (machineData is IMoveLoadingUnitMachineData machineMoveData)
            {
                this.moveData = machineMoveData;

                var direction = HorizontalMovementDirection.Backwards;
                bool measure = false;
                switch (this.moveData.LoadingUnitSource)
                {
                    case LoadingUnitLocation.Cell:
                        if (this.moveData.LoadingUnitCellSourceId != null)
                        {
                            var cell = this.cellsProvider.GetById(this.moveData.LoadingUnitCellSourceId.Value);

                            direction = cell.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        }

                        break;

                    default:
                        var bay = this.baysDataProvider.GetByLoadingUnitLocation(this.moveData.LoadingUnitSource);
                        direction = bay.Side == WarehouseSide.Front ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
                        this.openShutter = (bay.Shutter.Type != ShutterType.NotSpecified);
                        measure = true;
                        break;
                }

                this.loadingUnitMovementProvider.MoveLoadingUnit(direction, true, this.openShutter, measure, MessageActor.MachineManager, commandMessage.RequestingBay, machineMoveData.LoadingUnitId);
                moveData.FsmStateName = this.GetType().Name;
            }
            else
            {
                var description = $"Move Loading Unit Load Unit Sate received wrong initialization data ({commandMessage.Data.GetType().Name})";

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
                        if (shutterPosition == ShutterPosition.Opened
                            || shutterPosition == ShutterPosition.NotSpecified)
                        {
                            this.loadingUnitMovementProvider.ContinuePositioning(MessageActor.MachineManager, notification.RequestingBay);
                        }
                        else
                        {
                            this.Logger.LogError(ErrorDescriptions.MachineManagerErrorLoadingUnitShutterClosed);
                            this.errorsProvider.RecordNew(MachineErrorCode.MachineManagerErrorLoadingUnitShutterClosed);

                            returnValue = this.GetState<IMoveLoadingUnitEndState>();

                            ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                            ((IEndState)returnValue).ErrorMessage = notification;
                        }
                    }

                    break;

                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                    returnValue = this.GetState<IMoveLoadingUnitEndState>();

                    ((IEndState)returnValue).StopRequestReason = StopRequestReason.Error;
                    ((IEndState)returnValue).ErrorMessage = notification;
                    break;
            }

            if ((this.openShutter && this.stateMachineResponses.Count == 2) || (!this.openShutter && this.stateMachineResponses.Count == 1))
            {
                using (var transaction = this.elevatorDataProvider.GetContextTransaction())
                {
                    this.elevatorDataProvider.SetLoadingUnit(this.moveData.LoadingUnitId);

                    if (this.moveData.LoadingUnitSource == LoadingUnitLocation.Cell)
                    {
                        var sourceCellId = this.moveData.LoadingUnitCellSourceId;
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
                        var bayPosition = this.baysDataProvider.GetPositionByLocation(this.moveData.LoadingUnitSource);
                        this.baysDataProvider.SetLoadingUnit(bayPosition.Id, null);
                    }

                    transaction.Commit();
                }

                returnValue = this.GetState<IMoveLoadingUnitMoveToTargetState>();
            }

            return returnValue;
        }

        protected override IState OnStop(StopRequestReason reason)
        {
            var returnValue = this.GetState<IMoveLoadingUnitEndState>();

            ((IEndState)returnValue).StopRequestReason = reason;

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
