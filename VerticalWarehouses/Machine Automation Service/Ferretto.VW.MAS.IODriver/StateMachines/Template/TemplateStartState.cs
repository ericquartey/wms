using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.IODriver.StateMachines.Template
{
    internal sealed class TemplateStartState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        private readonly ITemplateData templateData;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public TemplateStartState(
            ITemplateData templateData,
            IoStatus status,
            IoIndex index,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.templateData = templateData;
            this.status = status;
            this.index = index;
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            if (message.ValidOutputs)
            {
                this.ParentStateMachine.ChangeState(new TemplateEndState(this.templateData, this.status, this.index, this.Logger, this.ParentStateMachine));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new TemplateErrorState(this.templateData, this.status, this.index, this.Logger, this.ParentStateMachine));
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
                MessageStatus.OperationStart,
                (byte)this.index);

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
