using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
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

        private readonly IShutterPositioningStateMachineData shutterPositioningStateMachineData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(
            IStateMachine parentMachine,
            IShutterPositioningStateMachineData shutterPositioningStateMachineData)
            : base(parentMachine, shutterPositioningStateMachineData.Logger)
        {
            this.shutterPositioningStateMachineData = shutterPositioningStateMachineData;
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
                            if (this.shutterPositioningStateMachineData.PositioningMessageData.MovementMode == MovementMode.TestLoop)
                            {
                                if (messageData.ShutterPosition != ShutterPosition.Opened)
                                {
                                    this.Logger.LogError($"Shutter not in Opened position before Test Loop: {messageData.ShutterPosition}");
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningStateMachineData, message));
                                }
                                else
                                {
                                    // TestLoop: close shutter
                                    ShutterPosition shutterPositionTarget;
                                    shutterPositionTarget = ShutterPosition.Closed;
                                    if (this.shutterPositioningStateMachineData.PositioningMessageData.ShutterType == ShutterType.Shutter3Type)
                                    {
                                        shutterPositionTarget = ShutterPosition.Half;
                                    }
                                    var commandData = new ShutterPositioningFieldMessageData(
                                        shutterPositionTarget,
                                        ShutterMovementDirection.Down,
                                        this.shutterPositioningStateMachineData.PositioningMessageData.ShutterType,
                                        this.shutterPositioningStateMachineData.PositioningMessageData.SpeedRate);

                                    var commandMessage = new FieldCommandMessage(
                                        commandData,
                                        $"Shutter to {shutterPositionTarget}",
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageType.ShutterPositioning,
                                        (byte)this.shutterPositioningStateMachineData.InverterIndex);

                                    this.Logger.LogDebug($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                                    this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

                                    this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningStateMachineData));
                                }
                            }
                            else
                            {
                                this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningStateMachineData));
                            }
                        }
                        break;

                    case MessageStatus.OperationStart:
                        if (this.shutterPositioningStateMachineData.PositioningMessageData.MovementMode == MovementMode.Position)
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningStateMachineData));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningStateMachineData, message));
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

            inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.Logger.LogTrace($"3:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            ShutterPositioningFieldMessageData messageData;
            if (this.shutterPositioningStateMachineData.PositioningMessageData.MovementMode == MovementMode.Position)
            {
                messageData = new ShutterPositioningFieldMessageData(this.shutterPositioningStateMachineData.PositioningMessageData);
            }
            else
            {
                // TestLoop: first move the shutter in Open position
                messageData = new ShutterPositioningFieldMessageData(
                    ShutterPosition.Opened,
                    ShutterMovementDirection.Up,
                    this.shutterPositioningStateMachineData.PositioningMessageData.ShutterType,
                    this.shutterPositioningStateMachineData.PositioningMessageData.SpeedRate);
            }

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Start shutter positioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)this.shutterPositioningStateMachineData.InverterIndex);

            this.Logger.LogDebug($"4:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningStateMachineData.PositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.shutterPositioningStateMachineData.InverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.shutterPositioningStateMachineData.InverterIndex * inverterStatus.aglInverterInputs.Length);
            Array.Copy(this.shutterPositioningStateMachineData.MachineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Get shutter status",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.shutterPositioningStateMachineData.RequestingBay,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"5:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningStateMachineData, true));
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
