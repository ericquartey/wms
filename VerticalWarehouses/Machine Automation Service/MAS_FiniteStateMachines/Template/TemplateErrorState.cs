using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines.Template
{
    public class TemplateErrorState : StateBase
    {
        #region Fields

        private readonly Axis currentAxis;

        private readonly FieldNotificationMessage errorMessage;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateErrorState(IStateMachine parentMachine, Axis currentAxis, FieldNotificationMessage errorMessage, ILogger logger)
        {
            logger.LogDebug( "1:Method Start" );
            this.logger = logger;

            this.ParentStateMachine = parentMachine;
            this.currentAxis = currentAxis;
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Destructors

        ~TemplateErrorState()
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

            this.logger.LogTrace( $"2:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}" );

            if (message.Type == FieldMessageType.InverterPowerOff && message.Status != MessageStatus.OperationStart)
            {
                var notificationMessageData = new HomingMessageData( this.currentAxis, MessageVerbosity.Error );
                var notificationMessage = new NotificationMessage(
                    notificationMessageData,
                    "Homing Stopped due to an error",
                    MessageActor.Any,
                    MessageActor.FiniteStateMachines,
                    MessageType.Homing,
                    MessageStatus.OperationError,
                    ErrorLevel.Error );

                this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
            }
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

            //TODO Identify Operation Target Inverter
            var stopMessageData = new InverterStopFieldMessageData( InverterIndex.MainInverter );
            var stopMessage = new FieldCommandMessage( stopMessageData,
                $"Reset Inverter Axis {this.currentAxis}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterPowerOff );

            this.logger.LogTrace( $"2:Publish Field Command Message processed: {stopMessage.Type}, {stopMessage.Destination}" );

            this.ParentStateMachine.PublishFieldCommandMessage( stopMessage );
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
