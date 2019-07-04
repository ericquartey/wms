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
    public class ShutterControlOpenState : StateBase
    {
        #region Fields

        private readonly IShutterControlMessageData shutterControlMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterControlOpenState(
            IStateMachine parentMachine,
            IShutterControlMessageData shutterControlMessageData,
            ILogger logger,
            bool stopRequested = false)
            : base(parentMachine, logger)
        {
            this.shutterControlMessageData = shutterControlMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterControlOpenState()
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
                            switch (s.ShutterPosition)
                            {
                                case ShutterPosition.Half:
                                    this.ParentStateMachine.ChangeState(new ShutterControlHalfOpenState(this.ParentStateMachine, this.shutterControlMessageData, ShutterMovementDirection.Down, this.Logger));
                                    break;

                                case ShutterPosition.Closed:
                                    this.ParentStateMachine.ChangeState(new ShutterControlCloseState(this.ParentStateMachine, this.shutterControlMessageData, this.Logger));
                                    break;

                                case ShutterPosition.Opened:
                                case ShutterPosition.Undefined:
                                    var errorMessage = new FieldNotificationMessage(
                                        null,
                                        $"Invalid position of shutter",
                                        FieldMessageActor.FiniteStateMachines,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.ShutterPositioning,
                                        MessageStatus.OperationError);
                                    this.ParentStateMachine.ChangeState(new ShutterControlErrorState(this.ParentStateMachine, this.shutterControlMessageData, errorMessage, this.Logger));
                                    break;

                                case ShutterPosition.None:
                                    break;
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
            //TEMP Send a command to move shutter to:
            // - Close position, if shutter is of type Shutter2Type
            // - Half-open position, if shutter is of type Shutter3Type
            var shutterPositionTarget = ShutterPosition.None;
            switch (this.shutterControlMessageData.ShutterType)
            {
                case ShutterType.Shutter2Type:
                    shutterPositionTarget = ShutterPosition.Closed;
                    break;

                case ShutterType.Shutter3Type:
                    shutterPositionTarget = ShutterPosition.Half;
                    break;

                case ShutterType.NoType:
                    break;
            }

            var messageData = new ShutterPositioningFieldMessageData(
                shutterPositionTarget,
                Common_Utils.Messages.Enumerations.ShutterMovementDirection.Down,
                this.shutterControlMessageData.ShutterType,
                this.shutterControlMessageData.SpeedRate);

            var commandMessage = new FieldCommandMessage(
                messageData,
                $"Shutter to {shutterPositionTarget}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessage = new NotificationMessage(
                this.shutterControlMessageData,
                "ShutterControl Test Executing",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterControl,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        /// <inheritdoc/>
        public override void Stop()
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
