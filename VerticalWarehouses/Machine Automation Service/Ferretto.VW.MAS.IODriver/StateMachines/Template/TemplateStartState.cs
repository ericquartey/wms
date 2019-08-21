using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.Interface;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Template
{
    public class TemplateStartState : IoStateBase
    {

        #region Fields

        private readonly IoStatus status;

        private readonly ITemplateData templateData;

        private bool disposed;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public TemplateStartState(
            ITemplateData templateData,
            IoStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.templateData = templateData;
            this.status = status;
        }

        #endregion

        #region Destructors

        ~TemplateStartState()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

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

            base.Dispose(disposing);
        }

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs)
            {
                this.ParentStateMachine.ChangeState(new TemplateEndState(this.templateData, this.status, this.Logger, this.ParentStateMachine));
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            if (message.ValidOutputs)
            {
                this.ParentStateMachine.ChangeState(new TemplateEndState(this.templateData, this.status, this.Logger, this.ParentStateMachine));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new TemplateErrorState(this.templateData, this.status, this.Logger, this.ParentStateMachine));
            }
        }

        public override void Start()
        {
            var switchOffAxisIoMessage = new IoWriteMessage();

            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOffAxisIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(switchOffAxisIoMessage);

            var endNotification = new FieldNotificationMessage(
                null,
                "Template Start State",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.NoType,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
