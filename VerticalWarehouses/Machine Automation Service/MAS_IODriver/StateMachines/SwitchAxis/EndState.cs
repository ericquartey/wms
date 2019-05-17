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

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(Axis axisToSwitchOn, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.axisToSwitchOn = axisToSwitchOn;

            //var messageData = new SwitchAxisFieldMessageData(axisToSwitchOn, MessageVerbosity.Info);
            //var endNotification = new FieldNotificationMessage(messageData, "Motor Switch complete", FieldMessageActor.Any,
            //    FieldMessageActor.IoDriver, FieldMessageType.SwitchAxis, MessageStatus.OperationEnd);

            //this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            //this.ParentStateMachine.PublishNotificationEvent(endNotification);

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~EndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        // Useless
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
            this.logger.LogDebug("1:Method Start");

            var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
            var endNotification = new FieldNotificationMessage(messageData, "Motor Switch complete", FieldMessageActor.Any,
                FieldMessageActor.IoDriver, FieldMessageType.SwitchAxis, MessageStatus.OperationEnd);

            this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);

            this.logger.LogDebug("3:Method End");
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
