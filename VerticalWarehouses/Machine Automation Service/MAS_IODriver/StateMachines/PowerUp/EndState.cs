using Ferretto.VW.Common_Utils.Enumerations;
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

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.parentStateMachine = parentStateMachine;

            var resetSecurityIoMessage = new IoMessage(false);

            this.logger.LogTrace($"2:{resetSecurityIoMessage}");

            resetSecurityIoMessage.SwitchElevatorMotor(true);

            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~EndState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                var endNotification = new FieldNotificationMessage(null, "I/O power up complete", FieldMessageActor.Any,
                    FieldMessageActor.IoDriver, FieldMessageType.IoPowerUp, MessageStatus.OperationEnd);

                this.logger.LogTrace($"2:{endNotification.Type}:{endNotification.Destination}:{endNotification.Status}");

                this.parentStateMachine.PublishNotificationEvent(endNotification);
            }

            this.logger.LogDebug("3:End Start");
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
