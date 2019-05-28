using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningStartState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        private ShutterPosition shutterPosition;

        private ShutterMovementDirection shutterMovementDirection;

        private byte systemIndex;

        private decimal targetSpeed;

        private decimal acceleration;

        private decimal deceleration;

        #endregion

        #region Constructors

        public ShutterPositioningStartState(IStateMachine parentMachine, IShutterPositioningMessageData shutterPositioningMessageData, ILogger logger)
        {
            logger.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.shutterPositioningMessageData = shutterPositioningMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStartState()
        {
            this.Dispose( false );
        }

        #endregion

        /// <inheritdoc/>

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Command Message {message.Type} Source {message.Source}" );
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );
            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationStart:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningExecutingState(this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.Opened, this.logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState( new ShutterPositioningErrorState( this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.None, message, this.logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            var messageData = new ShutterPositioningFieldMessageData(this.shutterPosition, this.shutterMovementDirection, this.systemIndex, this.targetSpeed, this.acceleration, this.deceleration, MessageVerbosity.Info);

            var commandMessage = new FieldCommandMessage(messageData,
                $"Get shutter status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning );

            this.logger.LogTrace( $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}" );

            this.ParentStateMachine.PublishFieldCommandMessage( commandMessage );

            var notificationMessageData = new ShutterPositioningMessageData(this.shutterMovementDirection, this.shutterPositioningMessageData.BayNumber, this.targetSpeed, this.acceleration, this.deceleration, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Get shutter status",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationStart );

            this.logger.LogTrace( $"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}" );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );

            this.ParentStateMachine.ChangeState( new ShutterPositioningEndState( this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.None, this.logger, true ) );
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            base.Dispose( disposing );
        }

        #endregion
    }
}
