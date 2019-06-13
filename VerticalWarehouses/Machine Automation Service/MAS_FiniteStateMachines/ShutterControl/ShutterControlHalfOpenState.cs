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
    public class ShutterControlHalfOpenState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly ShutterMovementDirection movementDirection;

        private readonly IShutterControlMessageData shutterControlMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterControlHalfOpenState(IStateMachine parentMachine, IShutterControlMessageData shutterControlMessageData, Common_Utils.Messages.Enumerations.ShutterMovementDirection movementDirection, ILogger logger, bool stopRequested = false)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.shutterControlMessageData = shutterControlMessageData;
            this.movementDirection = movementDirection;
        }

        #endregion

        #region Destructors

        ~ShutterControlHalfOpenState()
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
                            switch (s.ShutterPosition)
                            {
                                case ShutterPosition.Opened:
                                    this.ParentStateMachine.ChangeState(new ShutterControlOpenState(this.ParentStateMachine, this.shutterControlMessageData, this.logger));
                                    break;

                                case ShutterPosition.Closed:
                                    this.ParentStateMachine.ChangeState(new ShutterControlCloseState(this.ParentStateMachine, this.shutterControlMessageData, this.logger));
                                    break;

                                case ShutterPosition.Half:
                                case ShutterPosition.Undefined:
                                    var errorMessage = new FieldNotificationMessage(
                                        null,
                                        $"Invalid position of shutter",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.ShutterPositioning,
                                        MessageStatus.OperationError);
                                    this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, errorMessage, this.logger));
                                    break;

                                case ShutterPosition.None:
                                    break;
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
            // Send a command to move the shutter
            // - Open position, if movementDirection field is Up
            // - Close position, if movementDirection field is Down

            var shutterPositionTarget = ShutterPosition.None;
            switch (this.movementDirection)
            {
                case ShutterMovementDirection.Down:
                    shutterPositionTarget = ShutterPosition.Closed;
                    break;

                case ShutterMovementDirection.Up:
                    shutterPositionTarget = ShutterPosition.Opened;
                    break;
            }

            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                this.movementDirection,
                this.shutterControlMessageData.ShutterType,
                this.shutterControlMessageData.SpeedRate);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning);

            this.logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);
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
