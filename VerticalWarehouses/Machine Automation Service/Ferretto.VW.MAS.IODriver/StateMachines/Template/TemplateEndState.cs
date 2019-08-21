using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Template
{
    public class TemplateEndState : IoStateBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateEndState(
            ITemplateData templateData,
            IoSHDStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine )
            : base( parentStateMachine, logger )
        {
            this.status = status;
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

        public override void ProcessMessage( IoSHDMessage message )
        {
            //INFO This method should never be used in an error state
            this.Logger.LogTrace( $"1:Message processed: {message}" );
        }

        public override void ProcessResponseMessage( IoSHDReadMessage message )
        {
            //INFO This method should never be used in an error state
            this.Logger.LogTrace( $"1:Message processed: {message}" );
        }

        public override void Start()
        {
            var endNotification = new FieldNotificationMessage(
                null,
                "Template End State",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.NoType,
                MessageStatus.OperationEnd );

            this.ParentStateMachine.PublishNotificationEvent( endNotification );
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
