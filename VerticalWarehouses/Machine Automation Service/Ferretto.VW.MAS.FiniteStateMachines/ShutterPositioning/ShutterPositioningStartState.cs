using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
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

        private readonly Timer delayTimer;

        private readonly InverterIndex inverterIndex;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(
            IStateMachine parentMachine,
            IShutterPositioningMessageData shutterPositioningMessageData,
            InverterIndex inverterIndex,
            ILogger logger,
            IMachineSensorsStatus machineSensorsStatus,
            Timer delayTimer)
            : base(parentMachine, logger)
        {
            this.shutterPositioningMessageData = shutterPositioningMessageData;

            this.inverterIndex = inverterIndex;

            this.machineSensorsStatus = machineSensorsStatus;

            this.delayTimer = delayTimer;
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
                            if (this.shutterPositioningMessageData.MovementMode == MovementMode.TestLoop)
                            {
                                if (messageData.ShutterPosition != ShutterPosition.Opened)
                                {
                                    this.Logger.LogError($"Shutter not in Opened position before Test Loop: {messageData.ShutterPosition}");
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, message, this.Logger));
                                }
                                else
                                {
                                    // TestLoop: close shutter
                                    ShutterPosition shutterPositionTarget;
                                    shutterPositionTarget = ShutterPosition.Closed;
                                    if (this.shutterPositioningMessageData.ShutterType == ShutterType.Shutter3Type)
                                    {
                                        shutterPositionTarget = ShutterPosition.Half;
                                    }
                                    var commandData = new ShutterPositioningFieldMessageData(
                                        shutterPositionTarget,
                                        ShutterMovementDirection.Down,
                                        this.shutterPositioningMessageData.ShutterType,
                                        this.shutterPositioningMessageData.SpeedRate);

                                    var commandMessage = new FieldCommandMessage(
                                        commandData,
                                        $"Shutter to {shutterPositionTarget}",
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageType.ShutterPositioning,
                                        (byte)this.inverterIndex);

                                    this.Logger.LogDebug($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

                                    this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

                                    this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.delayTimer, this.Logger));
                                }
                            }
                            else
                            {
                                this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger));
                            }
                        }
                        break;

                    case MessageStatus.OperationStart:
                        if (this.shutterPositioningMessageData.MovementMode == MovementMode.Position)
                        {
                            this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.delayTimer, this.Logger));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, message, this.Logger));
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
            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, false, 0);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSetTimer,
                (byte)InverterIndex.MainInverter);
            this.Logger.LogTrace($"3:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.inverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.inverterIndex * inverterStatus.aglInverterInputs.Length);
            Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;

            ShutterPositioningFieldMessageData messageData;
            if (this.shutterPositioningMessageData.MovementMode == MovementMode.Position)
            {
                messageData = new ShutterPositioningFieldMessageData(this.shutterPositioningMessageData);
            }
            else
            {
                // TestLoop:
                // not all starting positions are allowed
                if (this.shutterPositioningMessageData.ShutterType == ShutterType.Shutter3Type &&
                    (inverterStatus.CurrentShutterPosition == ShutterPosition.Intermediate || inverterStatus.CurrentShutterPosition == ShutterPosition.Closed)
                    )
                {
                    this.Logger.LogError($"Shutter in Intermediate position before Test Loop");
                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, null, this.Logger));
                    return;
                }
                // first move the shutter in Open position
                messageData = new ShutterPositioningFieldMessageData(
                    ShutterPosition.Opened,
                    ShutterMovementDirection.Up,
                    this.shutterPositioningMessageData.ShutterType,
                    this.shutterPositioningMessageData.SpeedRate);
            }

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Start shutter positioning",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)this.inverterIndex);

            this.Logger.LogDebug($"4:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Get shutter status",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"5:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger, true));
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
