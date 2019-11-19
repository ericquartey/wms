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
                case FieldMessageType.InverterSwitchOn:
                case FieldMessageType.InverterSwitchOff:
                    switch (message.Status)
                    {
                        case MessageStatus.OperationStop:
                        case MessageStatus.OperationEnd:

                            //if (message.Status == MessageStatus.OperationEnd && this.machineData.Requester == MessageActor.AutomationService && this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                            //{
                            //    this.UpdateLoadingUnitLocation();
                            //}

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
            this.Logger?.LogTrace("1:Method Start");

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
                if (this.machineData.Requester == MessageActor.AutomationService && this.machineData.MessageData.AxisMovement == Axis.Horizontal)
                {
                    this.UpdateLoadingUnitLocation();
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

        private void UpdateLoadingUnitLocation()
        {
            LoadingUnit currentLoadingUnit;

            var serviceProvider = this.ParentStateMachine.ServiceScopeFactory.CreateScope().ServiceProvider;

            var resourceProvider = serviceProvider.GetRequiredService<IMachineResourcesProvider>();

            var elevatorDataProvider = serviceProvider.GetRequiredService<IElevatorDataProvider>();

            var elevatorProvider = serviceProvider.GetRequiredService<IElevatorProvider>();

            var bayProvider = serviceProvider.GetRequiredService<IBaysProvider>();

            var cellProvider = serviceProvider.GetRequiredService<ICellsProvider>();

            var bayLocation = LoadingUnitLocation.NoLocation;

            var position = elevatorProvider.VerticalPosition;

            var side = (this.machineData.MessageData.IsStartedOnBoard ?
                (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards ? WarehouseSide.Front : WarehouseSide.Back) :
                (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards ? WarehouseSide.Front : WarehouseSide.Back));

            var cell = cellProvider.GetCellByHeight(position, 10, side);
            if (cell == null)
            {
                bayLocation = bayProvider.GetPositionByHeight(position, 10, this.machineData.RequestingBay);
            }

            currentLoadingUnit = elevatorDataProvider.GetLoadingUnitOnBoard();

            if (currentLoadingUnit == null)
            {
                if (cell != null)
                {
                    currentLoadingUnit = cell.LoadingUnit;
                }
                else
                {
                    currentLoadingUnit = bayProvider.GetLoadingUnitByDestination(bayLocation);
                }
            }

            if (currentLoadingUnit == null)
            {
                this.Logger.LogWarning($"Found no loading unit at position {position}");
            }
            else
            {
                using (var transaction = elevatorDataProvider.GetContextTransaction())
                {
                    if (resourceProvider.IsDrawerCompletelyOnCradle)
                    {
                        elevatorDataProvider.LoadLoadingUnit(currentLoadingUnit.Id);

                        if (cell != null)
                        {
                            cellProvider.UnloadLoadingUnit(cell.Id);
                        }
                        else if (bayLocation != LoadingUnitLocation.NoLocation)
                        {
                            bayProvider.UnloadLoadingUnit(bayLocation);
                        }
                    }
                    else
                    {
                        elevatorDataProvider.UnloadLoadingUnit();

                        if (cell != null)
                        {
                            cellProvider.LoadLoadingUnit(currentLoadingUnit.Id, cell.Id);
                        }
                        else if (bayLocation != LoadingUnitLocation.NoLocation)
                        {
                            bayProvider.LoadLoadingUnit(currentLoadingUnit.Id, bayLocation);
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        #endregion
    }
}
