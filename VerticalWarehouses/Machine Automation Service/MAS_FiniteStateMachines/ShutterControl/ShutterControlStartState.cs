using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterControl
{
    public class ShutterControlStartState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IShutterControlMessageData shutterControlMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterControlStartState(IStateMachine parentMachine, IShutterControlMessageData shutterControlMessageData, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
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
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        /// <inheritdoc/>
        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

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
                                this.ParentStateMachine.ChangeState(new ShutterControlOpenState(this.ParentStateMachine, this.shutterControlMessageData, this.logger));
                            }
                            else
                            {
                                //TEMP It is an error condition, shutter isn't at Open position
                                this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, message, this.logger));
                            }
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, message, this.logger));
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
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
                FieldMessageType.ShutterPositioning);

            this.logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                this.shutterControlMessageData,
                "ShutterControl Test Started",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterControl,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterControlEndState(this.ParentStateMachine, this.shutterControlMessageData, this.logger, true));
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
