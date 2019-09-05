using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterControl
{
    public class ShutterControlStartState : StateBase
    {

        #region Fields

        private readonly IShutterTestStatusChangedMessageData shutterControlMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterControlStartState(
            IStateMachine parentMachine,
            IShutterTestStatusChangedMessageData shutterControlMessageData,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.shutterControlMessageData = shutterControlMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterControlStartState()
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

        /// <inheritdoc/>
        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        // TEMP Do it need to make something?!
                        break;

                    case MessageStatus.OperationEnd:
                        if (message.Data is InverterShutterPositioningFieldMessageData s)
                        {
                            if (s.ShutterPosition == ShutterPosition.Opened)
                            {
                                this.ParentStateMachine.ChangeState(new ShutterControlOpenState(this.ParentStateMachine, this.shutterControlMessageData, this.Logger));
                            }
                            else
                            {
                                //TEMP It is an error condition, shutter isn't at Open position
                                this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, message, this.Logger));
                            }
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, message, this.Logger));
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
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
            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            //TEMP Start means:
            // Send a InverterShutterPositioningMessage to move the shutter in Open position, before start the cycles
            var messageData = new ShutterPositioningFieldMessageData(
                ShutterPosition.Opened,
                ShutterMovementDirection.Up,
                this.shutterControlMessageData.ShutterType,
                this.shutterControlMessageData.SpeedRate);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {ShutterPosition.Opened}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning,
                (byte)InverterIndex.Slave2);

            this.Logger.LogTrace($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                this.shutterControlMessageData,
                "ShutterControl Test Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterTestStatusChanged,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        /// <param name="reason"></param>
        /// <inheritdoc/>
        public override void Stop(StopRequestReason reason = StopRequestReason.Stop)
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterControlEndState(this.ParentStateMachine, this.shutterControlMessageData, this.Logger, true));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
