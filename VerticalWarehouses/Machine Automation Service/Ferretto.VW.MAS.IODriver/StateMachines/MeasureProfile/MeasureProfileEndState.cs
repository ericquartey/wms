using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.IODriver.StateMachines.MeasureProfile
{
    internal sealed class MeasureProfileEndState : IoStateBase
    {
        #region Fields

        private readonly IoIndex deviceIndex;

        private readonly bool enable;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public MeasureProfileEndState(
            bool enable,
            IoStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine,
            IoIndex deviceIndex)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.enable = enable;

            this.deviceIndex = deviceIndex;

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
                "Measure Profile complete",
                FieldMessageActor.IoDriver,
                FieldMessageActor.IoDriver,
                FieldMessageType.MeasureProfile,
                MessageStatus.OperationEnd,
                (byte)this.deviceIndex);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
