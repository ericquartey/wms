using System;
using System;
using System.Linq;
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

        private readonly IErrorsProvider errorsProvider;

        private readonly IPositioningMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IPositioningStateData stateData;

        #endregion

        #region Constructors

        public PositioningEndState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
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
                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterSwitchOff:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:

                            if (message.Status is MessageStatus.OperationEnd
                                &&
                                this.machineData.MessageData.AxisMovement is Axis.Horizontal)
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
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InverterErrorBaseCode, this.machineData.RequestingBay);
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
            if (this.machineData.MessageData.AxisMovement is Axis.Horizontal)
            {
                this.UpdateLoadingUnitLocation();
            }

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
                if (this.machineData.MessageData.AxisMovement is Axis.Vertical)
                {
                    this.PersistElevatorPosition(
                        this.machineData.MessageData.TargetBayPositionId,
                        this.machineData.MessageData.TargetCellId,
                        this.machineData.MessageData.TargetPosition);
                }
                else if (this.machineData.MessageData.AxisMovement is Axis.Horizontal
                    && this.machineData.MessageData.MovementType == MovementType.TableTarget
                    )
                {
                    this.UpdateLastIdealPosition();
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
            System.Diagnostics.Debug.Assert(targetBayPositionId.HasValue || targetCellId.HasValue);

            var baysProvider = serviceProvider.GetRequiredService<IBaysProvider>();
            var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();
            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();

            var loadingUnitOnBoard = elevatorDataProvider.GetLoadingUnitOnBoard();
            if (loadingUnitOnBoard is null)
            {
                return;
            }

            if (loadingUnitOnBoard.Id != loadingUnitId)
            {
                throw new InvalidOperationException(
                    $"The loading unit on board of the elevator (id={loadingUnitOnBoard.Id}) is not the same loading unit requested for deposit (id={loadingUnitId}).");
            }

            if (targetBayPositionId.HasValue)
            {
                baysProvider.SetLoadingUnit(targetBayPositionId.Value, loadingUnitOnBoard.Id);
            }
            else if (targetCellId.HasValue)
            {
                cellsProvider.SetLoadingUnit(targetCellId.Value, loadingUnitOnBoard.Id);
            }

            elevatorDataProvider.SetLoadingUnit(null);
        }

        private static void UpdateLoadingUnitForPickup(int? sourceBayPositionId, int? sourceCellId, IServiceProvider serviceProvider)
        {
            System.Diagnostics.Debug.Assert(sourceBayPositionId.HasValue || sourceCellId.HasValue);

            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
            var baysProvider = serviceProvider.GetRequiredService<IBaysProvider>();
            var cellsProvider = serviceProvider.GetRequiredService<ICellsProvider>();

            var loadingUnitOnBoard = elevatorDataProvider.GetLoadingUnitOnBoard();
            if (loadingUnitOnBoard != null)
            {
                throw new InvalidOperationException(
                    $"A pickup was requested, but the elevator has already a loading unit (id={loadingUnitOnBoard.Id}) on board.");
            }

            if (sourceBayPositionId.HasValue)
            {
                var bayPosition = baysProvider.GetPositionById(sourceBayPositionId.Value);

                elevatorDataProvider.SetLoadingUnit(bayPosition.LoadingUnit?.Id);

                baysProvider.SetLoadingUnit(sourceBayPositionId.Value, null);
            }
            else if (sourceCellId.HasValue)
            {
                var cell = cellsProvider.GetById(sourceCellId.Value);

                elevatorDataProvider.SetLoadingUnit(cell.LoadingUnit?.Id);

                cellsProvider.SetLoadingUnit(sourceCellId.Value, null);
            }
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

        private void UpdateLastIdealPosition()
        {
            var serviceProvider = this.ParentStateMachine.ServiceScopeFactory.CreateScope().ServiceProvider;
            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();
            elevatorDataProvider.UpdateLastIdealPosition(this.machineData.MessageData.TargetPosition);
        }

        private void UpdateLoadingUnitForManualMovement()
        {
            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
            {
                var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                var baysProvider = scope.ServiceProvider.GetRequiredService<IBaysProvider>();
                var cellsProvider = scope.ServiceProvider.GetRequiredService<ICellsProvider>();
                var machineResourcesProvider = scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();

                var loadingUnitOnElevator = elevatorDataProvider.GetLoadingUnitOnBoard();

                // 1. check if elevator is opposite a bay or a cell
                var bayPosition = elevatorDataProvider.GetCurrentBayPosition();
                var cell = elevatorDataProvider.GetCurrentCell();

                if (bayPosition != null)
                {
                    var bay = baysProvider.GetByBayPositionId(bayPosition.Id);
                    var isDrawerInBay = bayPosition.IsUpper
                         ? machineResourcesProvider.IsDrawerInBayTop(bay.Number)
                         : machineResourcesProvider.IsDrawerInBayBottom(bay.Number);

                    if (loadingUnitOnElevator == null && bayPosition.LoadingUnit != null)
                    // possible pickup from bay
                    {
                        if (machineResourcesProvider.IsDrawerCompletelyOnCradle && !isDrawerInBay)
                        {
                            elevatorDataProvider.SetLoadingUnit(bayPosition.LoadingUnit.Id);
                            baysProvider.SetLoadingUnit(bayPosition.Id, null);
                        }
                    }
                    else if (loadingUnitOnElevator != null && bayPosition.LoadingUnit == null)
                    // possible deposit to bay
                    {
                        if (machineResourcesProvider.IsDrawerCompletelyOffCradle && isDrawerInBay)
                        {
                            elevatorDataProvider.SetLoadingUnit(null);
                            baysProvider.SetLoadingUnit(bayPosition.Id, loadingUnitOnElevator.Id);
                        }
                    }
                }
                else if (cell != null)
                {
                    if (loadingUnitOnElevator == null && cell.LoadingUnit != null)
                    // possible pickup from cell
                    {
                        if (machineResourcesProvider.IsDrawerCompletelyOnCradle)
                        {
                            elevatorDataProvider.SetLoadingUnit(cell.LoadingUnit.Id);
                            cellsProvider.SetLoadingUnit(cell.Id, null);
                        }
                    }
                    else if (loadingUnitOnElevator != null && cell.LoadingUnit == null)
                    // possible deposit to cell
                    {
                        if (machineResourcesProvider.IsDrawerCompletelyOffCradle)
                        {
                            if (cellsProvider.CanFitLoadingUnit(cell.Id, loadingUnitOnElevator.Id))
                            {
                                elevatorDataProvider.SetLoadingUnit(null);
                                cellsProvider.SetLoadingUnit(cell.Id, loadingUnitOnElevator.Id);
                            }
                            else
                            {
                                this.Logger.LogWarning("Detected loading unit leaving the cradle, but cell cannot store it.");
                            }
                        }
                    }
                }
            }
        }

        private void UpdateLoadingUnitLocation()
        {
            if (this.machineData.MessageData.LoadingUnitId.HasValue)
            {
                if (this.machineData.MessageData.SourceCellId.HasValue
                    ||
                    this.machineData.MessageData.SourceBayPositionId.HasValue)
                {
                    using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
                    {
                        UpdateLoadingUnitForPickup(
                            this.machineData.MessageData.SourceBayPositionId,
                            this.machineData.MessageData.SourceCellId,
                            scope.ServiceProvider);
                    }
                }
                else
                if (this.machineData.MessageData.TargetCellId.HasValue
                    ||
                    this.machineData.MessageData.TargetBayPositionId.HasValue)
                {
                    using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
                    {
                        UpdateLoadingUnitForDeposit(
                            this.machineData.MessageData.LoadingUnitId.Value,
                            this.machineData.MessageData.TargetBayPositionId,
                            this.machineData.MessageData.TargetCellId,
                            scope.ServiceProvider);
                    }
                }
                else
                {
                    this.Logger.LogWarning("No source or target cell/bay was specified for the horizontal positioning.");
                }
            }
            else
            {
                this.UpdateLoadingUnitForManualMovement();
            }
        }

        #endregion
    }
}
