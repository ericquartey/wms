using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    public class TemplateEndState : StateBase
    {
        #region Fields

        private readonly bool stopRequested;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateEndState(
            IStateMachine parentMachine,
            ITemplateData templateData,
            ILogger logger,
            bool stopRequested = false )
            : base( parentMachine, logger )
        {
            this.stopRequested = stopRequested;
            this.templateData = templateData;
        }

        #endregion

        #region Destructors

        ~TemplateEndState()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage( CommandMessage message )
        {
        }

        public override void ProcessFieldNotificationMessage( FieldNotificationMessage message )
        {
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage( NotificationMessage message )
        {
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                null,
                "Homing Completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                this.stopRequested ? MessageStatus.OperationStop : MessageStatus.OperationEnd );

            this.Logger.LogTrace( $"1:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}" );

            this.ParentStateMachine.PublishNotificationMessage( notificationMessage );
        }

        public override void Stop()
        {
        }

        protected override void Dispose( bool disposing )
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
