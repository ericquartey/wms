using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningEndState : StateBase
    {
        #region Fields

        private readonly ILogger logger;

        //private readonly int shutterPositionMovement;

        private readonly ShutterPosition shutterPosition;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private readonly bool stopRequested;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningEndState(IStateMachine parentMachine, IShutterPositioningMessageData shutterPositioningMessageData, ShutterPosition shutterPosition, ILogger logger, bool stopRequested = false)
        {
            logger.LogDebug( "1:Method Start" );

            this.logger = logger;
            this.stopRequested = stopRequested;
            this.ParentStateMachine = parentMachine;
            this.shutterPosition = shutterPosition;
            this.shutterPositioningMessageData = shutterPositioningMessageData;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningEndState()
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

            if (message is IShutterPositioningFieldMessageData data)
            {
                var notificationMessageData = new ShutterPositioningMessageData( data.ShutterPosition );

                var notificationMessage = new NotificationMessage( notificationMessageData, "Current shutter position", MessageActor.WebApi, MessageActor.FiniteStateMachines, MessageType.ShutterPositioning, MessageStatus.OperationEnd );

                this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
            }

            this.logger.LogTrace( $"2:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}" );
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.logger.LogDebug( "1:Method Start" );

            this.logger.LogTrace( $"2:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}" );
        }

        public override void Start()
        {
            this.logger.LogDebug( "1:Method Start" );

            var notificationMessageData = new ShutterPositioningMessageData( this.shutterPositioningMessageData.ShutterPositionMovement, MessageVerbosity.Info );
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Shutter Positioning Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd );

            this.logger.LogTrace( $"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}" );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
            this.logger.LogDebug( "1:Method Start" );
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
            base.Dispose( disposing );
        }

        #endregion
    }
}
