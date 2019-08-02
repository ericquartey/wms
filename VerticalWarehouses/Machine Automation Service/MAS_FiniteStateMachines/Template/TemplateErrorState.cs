using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    public class TemplateErrorState : StateBase
    {
        #region Fields

        private readonly FieldNotificationMessage errorMessage;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateErrorState(
            IStateMachine parentMachine,
            ITemplateData templateData,
            FieldNotificationMessage errorMessage,
            ILogger logger )
            : base( parentMachine, logger )
        {
            this.templateData = templateData;
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Destructors

        ~TemplateErrorState()
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
                "Template Error State",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                MessageStatus.OperationError,
                ErrorLevel.Error );

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
