using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningEndState : StateBase
    {
        #region Fields

        private readonly IPositioningMachineData machineData;

        private readonly IPositioningStateData stateData;

        #endregion

        #region Constructors

        public PositioningEndState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");

            switch (message.Type)
            {
                case FieldMessageType.InverterStop:
                case FieldMessageType.Positioning:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:

                            if (message.Status == MessageStatus.OperationEnd
                                &&
                                this.machineData.Requester == MessageActor.AutomationService
                                &&
                                this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                            {
                                this.UpdateLoadingUnitLocation();
                            }

                            var notificationMessage = new NotificationMessage(
                                this.machineData.MessageData,
                                this.machineData.MessageData.RequiredCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                                MessageActor.DeviceManager,
                                MessageActor.DeviceManager,
                                MessageType.Positioning,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            break;

                        case MessageStatus.OperationError:
                            this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                            break;
                    }
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterIndex = this.machineData.CurrentInverterIndex;
            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                var stopMessage = new FieldCommandMessage(
                    null,
                    this.machineData.MessageData.RequiredCycles == 0 ? "Positioning Stopped" : "Belt Burninshing Stopped",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterStop,
                    (byte)inverterIndex);

                this.ParentStateMachine.PublishFieldCommandMessage(stopMessage);
            }
            else
            {
                if (this.machineData.Requester == MessageActor.AutomationService
                    &&
                    this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                {
                    this.UpdateLoadingUnitLocation();
                }

                if (this.machineData.MessageData.AxisMovement == Axis.Vertical)
                {
                    this.PersistElevatorPosition(
                        this.machineData.MessageData.TargetBayPositionId,
                        this.machineData.MessageData.TargetCellId,
                        this.machineData.MessageData.TargetPosition);
                }

                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    this.machineData.MessageData.RequiredCycles == 0 ? "Positioning Completed" : "Belt Burninshing Completed",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Positioning End");
            }

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, SENSOR_UPDATE_SLOW);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)inverterIndex);
            this.Logger.LogTrace($"4:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("Retry Stop Command");
            this.Start();
        }

        private static void UpdateLoadingUnitForDeposit(int loadingUnitId, int? targetBayPositionId, int? targetCellId, IServiceProvider serviceProvider)
        {
            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
            var loadingUnitOnBoard = elevatorDataProvider.GetLoadingUnitOnBoard();

            System.Diagnostics.Debug.Assert(loadingUnitOnBoard != null);

            if (loadingUnitOnBoard.Id != loadingUnitId)
            {
                throw new InvalidOperationException(
                    $"The loading unit on board of the elevator (id={loadingUnitOnBoard.Id}) is not the same loading unit requested for deposit (id={loadingUnitId}).");
            }

            if (targetBayPositionId.HasValue)
            {
                var elevatorCurrentBayPositionId = elevatorDataProvider.GetCurrentBayPosition()?.Id;
                if (targetBayPositionId == elevatorCurrentBayPositionId)
                {
                    var baysProvider = serviceProvider.GetRequiredService<IBaysProvider>();

                    baysProvider.SetLoadingUnit(
                        targetBayPositionId.Value,
                        loadingUnitOnBoard.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"The target bay position for deposit (id={elevatorCurrentBayPositionId}) is not the same as the one requested by the positioning (id={targetBayPositionId}).");
                }
            }
            else if (targetCellId.HasValue)
            {
                var elevatorCurrentCellId = elevatorDataProvider.GetCurrentCell()?.Id;
                if (targetCellId == elevatorCurrentCellId)
                {
                    var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();

                    cellsProvider.SetLoadingUnit(
                        targetCellId.Value,
                        loadingUnitOnBoard.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"The target cell for deposit (id={elevatorCurrentCellId}) is not the same as the one requested by the positioning (id={targetCellId}).");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"The elevator has a loading unit on board, but the deposit request has no target bay or cell.");
            }

            elevatorDataProvider.SetLoadingUnit(null);
        }

        private static void UpdateLoadingUnitForPickup(int loadingUnitId, int? sourceBayPositionId, int? sourceCellId, IServiceProvider serviceProvider)
        {
            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
            if (elevatorDataProvider.GetLoadingUnitOnBoard() != null)
            {
                throw new InvalidOperationException(
                    $"A pickup was requested, but the elevator has already a loading unit on board.");
            }

            if (sourceBayPositionId.HasValue)
            {
                var baysProvider = serviceProvider.GetRequiredService<IBaysProvider>();

                var bayPosition = baysProvider.GetPositionById(sourceBayPositionId.Value);
                if (bayPosition.LoadingUnit?.Id == loadingUnitId)
                {
                    baysProvider.SetLoadingUnit(
                          sourceBayPositionId.Value,
                          null);
                }

                baysProvider.SetLoadingUnit(
                    sourceBayPositionId.Value,
                    null);
            }
            else if (sourceCellId.HasValue)
            {
                var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();

                var cell = cellsProvider.GetById(sourceCellId.Value);
                if (cell.LoadingUnit?.Id == loadingUnitId)
                {
                    cellsProvider.SetLoadingUnit(
                        sourceCellId.Value,
                        null);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"The source cell for pickup (id={sourceCellId}) contains a loading unit (id={cell.LoadingUnit?.Id}) that is different from the requested one (id={loadingUnitId}).");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"The pickup operation has no source bay or cell.");
            }

            serviceProvider
                .GetRequiredService<IElevatorDataProvider>()
                .SetLoadingUnit(loadingUnitId);
        }

        private void PersistElevatorPosition(int? targetBayPositionId, int? targetCellId, double targetPosition)
        {
            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            {
                var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

                elevatorDataProvider.SetCurrentBayPosition(targetBayPositionId);
                elevatorDataProvider.SetCurrentCell(targetCellId);
            }
        }

        private void UpdateLoadingUnitLocation()
        {
            if (!this.machineData.MessageData.LoadingUnitId.HasValue)
            {
                return;
            }

            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            {
                var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

                var loadingUnitOnBoard = elevatorDataProvider.GetLoadingUnitOnBoard();
                if (loadingUnitOnBoard is null)
                {
                    this.Logger.LogInformation("Update for pickup");

                    UpdateLoadingUnitForPickup(
                        this.machineData.MessageData.LoadingUnitId.Value,
                        this.machineData.MessageData.SourceBayPositionId,
                        this.machineData.MessageData.SourceCellId,
                        scope.ServiceProvider);
                }
                else
                {
                    this.Logger.LogInformation("Update for deposit");

                    UpdateLoadingUnitForDeposit(
                        this.machineData.MessageData.LoadingUnitId.Value,
                        this.machineData.MessageData.TargetBayPositionId,
                        this.machineData.MessageData.TargetCellId,
                        scope.ServiceProvider);
                }
            }
        }

        #endregion
    }
}
