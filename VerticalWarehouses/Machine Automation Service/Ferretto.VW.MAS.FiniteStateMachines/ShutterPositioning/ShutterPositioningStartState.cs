using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningStartState : StateBase
    {

        #region Fields

        private readonly IShutterPositioningMachineData machineData;

        private readonly IShutterPositioningStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(IShutterPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IShutterPositioningMachineData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
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
                                if (messageData.ShutterPosition != ShutterPosition.Opened)
                                {
                                    this.Logger.LogError($"Shutter not in Opened position before Test Loop: {messageData.ShutterPosition}");
                                    this.stateData.FieldMessage = message;
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.stateData));
                                }
                                else
                                {
                                    // first step: close shutter
                                    ShutterPosition shutterPositionTarget;
                                    shutterPositionTarget = ShutterPosition.Closed;
                                    if (this.machineData.PositioningMessageData.ShutterType == ShutterType.Shutter3Type)
                                    {
                                        shutterPositionTarget = ShutterPosition.Half;
                                    }
                                    var commandData = new ShutterPositioningFieldMessageData(
                                        shutterPositionTarget,
                                        ShutterMovementDirection.Down,
                                        this.machineData.PositioningMessageData.ShutterType,
                                        this.machineData.PositioningMessageData.SpeedRate);

                                    var commandMessage = new FieldCommandMessage(
                                        commandData,
                                        $"Shutter to {shutterPositionTarget}",
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageType.ShutterPositioning,
                                        (byte)this.machineData.InverterIndex);

                                    this.Logger.LogDebug($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

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
                        if (this.machineData.PositioningMessageData.MovementMode == MovementMode.Position)
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            // Send a field message to the Update of position axis to InverterDriver
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 50);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            ShutterPositioningFieldMessageData messageData;
            if (this.machineData.PositioningMessageData.MovementMode == MovementMode.Position)
            {
                messageData = new ShutterPositioningFieldMessageData(this.machineData.PositioningMessageData);
            }
            else
            {
                // TestLoop: first move the shutter in Open position
                messageData = new ShutterPositioningFieldMessageData(
                    ShutterPosition.Opened,
                    ShutterMovementDirection.Up,
                    this.machineData.PositioningMessageData.ShutterType,
                    this.machineData.PositioningMessageData.SpeedRate);
            }

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Start shutter positioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)this.machineData.InverterIndex);

            this.Logger.LogDebug($"4:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new ShutterPositioningMessageData(this.machineData.PositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.machineData.InverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.machineData.InverterIndex * inverterStatus.aglInverterInputs.Length);
            Array.Copy(this.machineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Get shutter status",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.machineData.RequestingBay,
                this.machineData.RequestingBay,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"5:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.stateData));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
