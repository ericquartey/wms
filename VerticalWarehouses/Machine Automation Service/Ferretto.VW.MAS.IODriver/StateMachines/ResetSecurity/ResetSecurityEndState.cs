using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.IODriver.StateMachines.ResetSecurity
{
    internal sealed class ResetSecurityEndState : IoStateBase
    {
        #region Fields

        private readonly bool hasError;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public ResetSecurityEndState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            IoIndex index,
            bool hasError,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.index = index;
            this.hasError = hasError;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Message processed: {message}");
        }

        public override void Start()
        {
            var endNotification = new FieldNotificationMessage(
                null,
                "Reset Security complete",
                FieldMessageActor.IoDriver,
                FieldMessageActor.IoDriver,
                FieldMessageType.ResetSecurity,
                this.hasError ? MessageStatus.OperationError : MessageStatus.OperationEnd,
                (byte)this.index,
                this.hasError ? ErrorLevel.Error : ErrorLevel.None);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
