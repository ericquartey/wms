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

        private readonly IoIndex index;

        private readonly ILogger logger;

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(IIoStateMachine parentStateMachine, IoSHDStatus status, IoIndex index, ILogger logger)
        {
            logger.LogTrace("1:Method Start");

            this.logger = logger;
            this.ParentStateMachine = parentStateMachine;
            this.status = status;
            this.index = index;
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
            this.logger.LogTrace("1:Method Start");

            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                var endNotification = new FieldNotificationMessage(
                    null,
                    "I/O power up complete",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.IoPowerUp,
                    MessageStatus.OperationEnd,
                    ErrorLevel.NoError,
                    (byte)this.index);

                this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.logger.LogDebug($"1: Received Message = {message.ToString()}");

            //TEMP Check the matching between the status output flags and the message output flags (i.e. the switch ElevatorMotorON has been processed)
            if (this.status.MatchOutputs(message.Outputs))
            {
                var endNotification = new FieldNotificationMessage(
                    null,
                    "I/O power up complete",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.IoPowerUp,
                    MessageStatus.OperationEnd,
                    ErrorLevel.NoError,
                    (byte)this.index);

                this.logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void Start()
        {
            var resetSecurityIoMessage = new IoSHDWriteMessage();

            resetSecurityIoMessage.SwitchElevatorMotor(true);
            this.logger.LogTrace($"1:Switch elevator MotorON IO={resetSecurityIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetSecurityIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(resetSecurityIoMessage);
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
