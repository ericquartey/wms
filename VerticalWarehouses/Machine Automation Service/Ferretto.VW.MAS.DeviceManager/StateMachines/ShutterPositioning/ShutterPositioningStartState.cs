using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning
{
    internal class ShutterPositioningStartState : StateBase
    {
        #region Fields

        private readonly IShutterPositioningMachineData machineData;

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        private readonly IServiceScope scope;

        private readonly IShutterPositioningStateData stateData;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(IShutterPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IShutterPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.machineModeDataProvider = this.scope.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (message.Data is InverterShutterPositioningFieldMessageData messageData)
                        {
                            if (this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterTest)
                            {
                                if (messageData.ShutterPosition == ShutterPosition.Half)
                                {
                                    ShutterPosition shutterPositionTarget;
                                    shutterPositionTarget = ShutterPosition.Opened;
                                    var commandData = new ShutterPositioningFieldMessageData(
                                        shutterPositionTarget,
                                        ShutterMovementDirection.Up,
                                        this.machineData.PositioningMessageData.ShutterType,
                                        -this.machineData.PositioningMessageData.SpeedRate,
                                        this.machineData.PositioningMessageData.HighSpeedDurationOpen,
                                        this.machineData.PositioningMessageData.HighSpeedDurationClose,
                                        -this.machineData.PositioningMessageData.LowerSpeed,
                                        this.machineData.PositioningMessageData.MovementType);

                                    var commandMessage = new FieldCommandMessage(
                                        commandData,
                                        $"Shutter to {shutterPositionTarget}",
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageType.ShutterPositioning,
                                        (byte)this.machineData.InverterIndex);

                                    this.Logger.LogDebug($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                                    this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
                                }
                                else if (messageData.ShutterPosition != ShutterPosition.Opened)
                                {
                                    this.Logger.LogError($"Shutter not in Opened position before Test Loop: {messageData.ShutterPosition}");
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                                }
                                else
                                {
                                    // first step: close shutter
                                    ShutterPosition shutterPositionTarget;
                                    shutterPositionTarget = ShutterPosition.Closed;
                                    if (this.machineData.PositioningMessageData.ShutterType == ShutterType.ThreeSensors)
                                    {
                                        shutterPositionTarget = ShutterPosition.Half;
                                    }
                                    var commandData = new ShutterPositioningFieldMessageData(
                                        shutterPositionTarget,
                                        ShutterMovementDirection.Down,
                                        this.machineData.PositioningMessageData.ShutterType,
                                        this.machineData.PositioningMessageData.SpeedRate,
                                        this.machineData.PositioningMessageData.HighSpeedDurationOpen,
                                        this.machineData.PositioningMessageData.HighSpeedDurationClose,
                                        this.machineData.PositioningMessageData.LowerSpeed,
                                        this.machineData.PositioningMessageData.MovementType);

                                    var commandMessage = new FieldCommandMessage(
                                        commandData,
                                        $"Shutter to {shutterPositionTarget}",
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageType.ShutterPositioning,
                                        (byte)this.machineData.InverterIndex);

                                    this.Logger.LogDebug($"3:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                                    this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

                                    this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.stateData));
                                }
                            }
                            else
                            {
                                this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData));
                            }
                        }
                        break;

                    case MessageStatus.OperationStart:
                        if (this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterPosition)
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.stateData));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.InverterIndex}");

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)this.machineData.InverterIndex);
            this.Logger.LogTrace($"3:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 250);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {InverterIndex.MainInverter}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
            var inverterStatus = new AglInverterStatus(this.machineData.InverterIndex);
            var sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.machineData.InverterIndex * inverterStatus.Inputs.Length);
            Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

            ShutterPositioningFieldMessageData messageData;
            if (this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterPosition)
            {
                // Absolute positioning: not all starting positions are allowed
                if (this.machineData.PositioningMessageData.MovementType == MovementType.Absolute &&
                    (inverterStatus.CurrentShutterPosition == ShutterPosition.Intermediate))
                {
                    this.Logger.LogError($"Shutter in Intermediate position before absolute positioning");
                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                    return;
                }
                messageData = new ShutterPositioningFieldMessageData(this.machineData.PositioningMessageData);
            }
            else
            {
                // TestLoop:
                // not all starting positions are allowed
                if (this.machineData.PositioningMessageData.ShutterType == ShutterType.ThreeSensors &&
                    (inverterStatus.CurrentShutterPosition == ShutterPosition.Intermediate)
                    )
                {
                    this.Logger.LogError($"Shutter in Intermediate position before Test Loop");
                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                    return;
                }
                this.machineModeDataProvider.Mode = MachineMode.Test;
                this.Logger.LogInformation($"Machine status switched to {MachineMode.Test}");

                // first move the shutter in Open position
                messageData = new ShutterPositioningFieldMessageData(
                    ShutterPosition.Opened,
                    ShutterMovementDirection.Up,
                    this.machineData.PositioningMessageData.ShutterType,
                    -this.machineData.PositioningMessageData.SpeedRate,
                    this.machineData.PositioningMessageData.HighSpeedDurationOpen,
                    this.machineData.PositioningMessageData.HighSpeedDurationClose,
                    -this.machineData.PositioningMessageData.LowerSpeed,
                    this.machineData.PositioningMessageData.MovementType);
            }

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Start shutter positioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.ShutterPositioning,
                (byte)this.machineData.InverterIndex);

            this.Logger.LogDebug($"4:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData1 = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
            var inverterStatus1 = new AglInverterStatus(this.machineData.InverterIndex);
            var sensorStart1 = (int)(IOMachineSensors.PowerOnOff + (int)this.machineData.InverterIndex * inverterStatus1.Inputs.Length);
            Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart1, inverterStatus1.Inputs, 0, inverterStatus1.Inputs.Length);
            notificationMessageData1.ShutterPosition = inverterStatus1.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData1,
                "Get shutter status",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.ShutterPositioning,
                this.machineData.RequestingBay,
                this.machineData.RequestingBay,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"5:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData));
        }

        #endregion
    }
}
