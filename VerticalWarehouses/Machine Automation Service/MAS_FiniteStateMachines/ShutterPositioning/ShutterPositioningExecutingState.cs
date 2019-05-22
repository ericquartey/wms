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
            logger.LogDebug( "1:Method Start " );

            this.logger = logger;
            this.ParentStateMachine = parentMachine;
            this.shutterPosition = shutterPosition;
            this.shutterPositioningMessageData = shutterPositioningMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningExecutingState()
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

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState( new ShutterPositioningEndState( this.ParentStateMachine, this.shutterPositioningMessageData, this.shutterPosition, this.logger ) );
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState( new ShutterPositioningErrorState( this.ParentStateMachine, this.shutterPositioningMessageData, ShutterPosition.None, message, this.logger ) );
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
            this.logger.LogDebug( "1:Method Start " );

            var commandMessageData = new ShutterPositioningFieldMessageData( this.shutterPosition, (byte)this.shutterPositioningMessageData.BayNumber ); // TODO BayNumber should natively be of byte type, has to be implemented yet on the state machine
            var commandMessage = new FieldCommandMessage( commandMessageData,
                $"Move to {shutterPosition}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.ShutterPositioning );

            this.logger.LogTrace( $"2:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}" );

            this.ParentStateMachine.PublishFieldCommandMessage( commandMessage );

            var notificationMessageData = new ShutterPositioningMessageData( this.shutterPositioningMessageData.ShutterPositionMovement, MessageVerbosity.Info );
            var notificationMessage = new NotificationMessage( notificationMessageData,
                $"Move {shutterPosition}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationExecuting );

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
