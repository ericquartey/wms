using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Template
{
    public class TemplateStateMachine : IoStateMachineBase
    {
        #region Fields

        private readonly IoSHDStatus status;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine( ITemplateData templateData,
            IoSHDStatus status,
            BlockingConcurrentQueue<IoSHDWriteMessage> ioCommandQueue,
            IEventAggregator eventAggregator,
            ILogger logger )
            : base( eventAggregator, logger )
        {
            this.templateData = templateData;
            this.IoCommandQueue = ioCommandQueue;
            this.status = status;
        }

        #endregion

        #region Destructors

        ~TemplateStateMachine()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.CurrentState = new TemplateStartState( this.templateData, this.status, this.Logger, this );

            var notificationMessage = new FieldNotificationMessage(
                null,
                $"Template Message",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.SwitchAxis,
                MessageStatus.OperationStart );

            this.PublishNotificationEvent( notificationMessage );

            this.CurrentState?.Start();
        }

        protected override void Dispose( bool disposing )
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose( disposing );
        }

        #endregion
    }
}
