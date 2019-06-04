using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class EndState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(Axis axisToSwitchOn, IoSHDStatus status, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogTrace("1:Method Start");
            this.logger = logger;

            this.status = status;
            this.ParentStateMachine = parentStateMachine;
            this.axisToSwitchOn = axisToSwitchOn;
        }

        #endregion

        #region Destructors

        ~EndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoSHDMessage message)
        {
            this.logger.LogTrace($"1:Message processed: {message}");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogTrace($"1:Message processed: {message}");
        }

        public override void Start()
        {
            var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
            var endNotification = new FieldNotificationMessage(messageData, "Motor Switch complete", FieldMessageActor.Any,
                FieldMessageActor.IoDriver, FieldMessageType.SwitchAxis, MessageStatus.OperationEnd);

            this.logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
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

            base.Dispose(disposing);
        }

        #endregion
    }
}
