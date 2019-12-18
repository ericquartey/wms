using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver.StateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.Template
{
    internal sealed class TemplateErrorState : IoStateBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        private readonly ITemplateData templateData;

        #endregion

        #region Constructors

        public TemplateErrorState(
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
            // INFO This method should never be used in an error state
            this.Logger.LogTrace($"1:Message processed: {message}");
        }

        public override void Start()
        {
            var endNotification = new FieldNotificationMessage(
                null,
                "Template Error State",
                FieldMessageActor.Any,
                FieldMessageActor.IoDriver,
                FieldMessageType.NoType,
                MessageStatus.OperationError,
                (byte)this.index);

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
