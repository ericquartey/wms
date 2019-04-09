using Ferretto.VW.MAS_IODriver.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
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
            this.ParentStateMachine = parentStateMachine;
            var resetSecurityIoMessage = new IoMessage(false);

            this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

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
            this.logger.LogTrace($"2:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}");

            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                this.logger.LogTrace("End State State ProcessMessage Notification Event");
                var endNotification = new FieldNotificationMessage(null, "IO Reset complete", FieldMessageActor.Any,
                    FieldMessageActor.IoDriver, FieldMessageType.IoReset, MessageStatus.OperationEnd);

                this.logger.LogTrace($"3:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }

            this.logger.LogDebug("4:Method End");
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
