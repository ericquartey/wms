using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class EndState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(IIoStateMachine parentStateMachine, IoSHDStatus status, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;

            /*var resetSecurityIoMessage = new IoSHDMessage(false);*/ // change with IoSHDWriteMessage
            //var resetSecurityIoMessage = new IoSHDWriteMessage();

            //this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

            //resetSecurityIoMessage.SwitchElevatorMotor(true);

            //parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

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
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                var endNotification = new FieldNotificationMessage(null, "I/O power up complete", FieldMessageActor.Any,
                    FieldMessageActor.IoDriver, FieldMessageType.IoPowerUp, MessageStatus.OperationEnd);

                this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }

            this.logger.LogDebug("3:End Start");
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.FormatDataOperation == Enumerations.SHDFormatDataOperation.Data &&
                message.ValidOutputs &&
                message.ElevatorMotorOn)
            {
                var endNotification = new FieldNotificationMessage(null, "I/O power up complete", FieldMessageActor.Any,
                    FieldMessageActor.IoDriver, FieldMessageType.IoPowerUp, MessageStatus.OperationEnd);

                this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }

            this.logger.LogDebug("3:End Start");
        }

        public override void Start()
        {
            this.logger.LogDebug("1:Method Start");

            var resetSecurityIoMessage = new IoSHDWriteMessage();

            this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

            resetSecurityIoMessage.SwitchElevatorMotor(true);

            this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);

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
