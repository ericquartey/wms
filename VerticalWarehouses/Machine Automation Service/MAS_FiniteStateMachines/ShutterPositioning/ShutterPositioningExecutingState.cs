using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningExecutingState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly ShutterPosition shutterPosition;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningExecutingState(IStateMachine parentMachine, IShutterPositioningMessageData shutterPositioningMessageData, ShutterPosition shutterPosition, ILogger logger)
        {
            logger.LogDebug("1:Method Start ");

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.shutterPosition = shutterPosition;
            this.shutterPositioningMessageData = shutterPositioningMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningExecutingState()
        {
            this.Dispose(false);
        }

        #endregion

        /// <inheritdoc/>

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.Opened, this.logger));
                        break;

                    //TEMP Maybe to be removed.
                    /* if (message.Data is IShutterPositioningFieldMessageData shutterData)
                     {
                         var newShutterPosition = ShutterPosition.None;
                         switch (shutterData.ShutterPosition)
                         {
                             case ShutterPosition.Opened:
                                 if (this.shutterPositioningMessageData.ShutterMovementDirection == ShutterMovementDirection.Up)
                                     this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.Opened, this.logger));
                                 else
                                 {
                                     switch (this.shutterPositioningMessageData.ShutterType)
                                     {
                                         case ShutterType.NoType:
                                             //TODO Notify Error
                                             break;

                                         case ShutterType.Shutter2Type:
                                             newShutterPosition = ShutterPosition.Closed;
                                             break;

                                         case ShutterType.Shutter3Type:
                                             newShutterPosition = ShutterPosition.Half;
                                             break;
                                     }
                                     this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningMessageData, newShutterPosition, this.logger));
                                 }
                                 break;

                             case ShutterPosition.Half:
                                 newShutterPosition = this.shutterPositioningMessageData.ShutterMovementDirection == ShutterMovementDirection.Up ? ShutterPosition.Opened : ShutterPosition.Closed;
                                 this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningMessageData, newShutterPosition, this.logger));
                                 break;

                             case ShutterPosition.Closed:
                                 if (this.shutterPositioningMessageData.ShutterMovementDirection == ShutterMovementDirection.Down)
                                     this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.Closed, this.logger));
                                 else
                                 {
                                     switch (this.shutterPositioningMessageData.ShutterType)
                                     {
                                         case ShutterType.NoType:
                                             break;

                                         case ShutterType.Shutter2Type:
                                             newShutterPosition = ShutterPosition.Opened;
                                             break;

                                         case ShutterType.Shutter3Type:
                                             newShutterPosition = ShutterPosition.Half;
                                             break;
                                     }
                                     this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningMessageData, newShutterPosition, this.logger));
                                 }
                                 break;

                             default:
                                 this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.None, message, this.logger));
                                 break;
                         }

                         var notificationMessage = new NotificationMessage(
                             this.shutterPositioningMessageData,
                             "Shutter positioning update notification",
                             MessageActor.Any,
                             MessageActor.FiniteStateMachines,
                             MessageType.ShutterPositioning,
                             MessageStatus.OperationExecuting
                             );
                         this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                     }
                     break;*/

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.None, message, this.logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var commandMessageData = new ShutterPositioningFieldMessageData(this.shutterPositioningMessageData);
            var commandMessage = new FieldCommandMessage(commandMessageData,
                $"Move to {this.shutterPosition}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning);

            this.logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningMessageData);
            var notificationMessage = new NotificationMessage(notificationMessageData,
                $"Move {this.shutterPosition}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationExecuting);

            this.logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.None, this.logger, true));
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
