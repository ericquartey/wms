using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningStartState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly int shutterPositionMovement;

        private bool disposed;

        private int shutterType;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(IStateMachine parentMachine, int shutterPositionMovement, ILogger logger, int shutterType)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;
            this.shutterType = shutterType;

            this.ParentStateMachine = parentMachine;
            this.shutterPositionMovement = shutterPositionMovement;

            var commandMessage = new FieldCommandMessage(null,
                $"Get shutter status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPosition);

            this.logger.LogTrace($"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositionMovement, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Get shutter status",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            this.logger.LogDebug("4:Method End");
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
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Command Message {message.Type} Source {message.Source}");

            this.logger.LogDebug("3:Method End");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            var shutterPosition = ShutterPosition.Unknown;

            if (message.Type == FieldMessageType.ShutterPosition)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (message.Data is IShutterPositionFieldMessageData shutterData)
                        {
                            switch (shutterData.ShutterPosition)
                            {
                                case ShutterPosition.Opened:
                                    if (this.shutterPositionMovement == 1)
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, ShutterPosition.Opened, this.logger));
                                    else
                                    {
                                        shutterPosition = this.shutterType == 1 ? ShutterPosition.Closed : ShutterPosition.Half;
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, shutterPosition, this.logger));
                                    }
                                    break;

                                case ShutterPosition.Half:
                                    shutterPosition = this.shutterPositionMovement == 1 ? ShutterPosition.Opened : ShutterPosition.Closed;
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, shutterPosition, this.logger));
                                    break;

                                case ShutterPosition.Closed:
                                    if (this.shutterPositionMovement == 0)
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, ShutterPosition.Closed, this.logger));
                                    else
                                    {
                                        shutterPosition = this.shutterType == 1 ? ShutterPosition.Opened : ShutterPosition.Half;
                                        this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, shutterPosition, this.logger));
                                    }
                                    break;

                                default:
                                    this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, ShutterPosition.Unknown, message, this.logger));
                                    break;
                            }
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, ShutterPosition.Unknown, message, this.logger));
                        break;
                }
            }
            this.logger.LogDebug("4:Method End");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            this.logger.LogTrace($"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            this.logger.LogDebug("3:Method End");
        }

        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, ShutterPosition.Unknown, this.logger, true));

            this.logger.LogDebug("2:Method End");
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
