using Ferretto.VW.Common_Utils.Messages.Enumerations;
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

        private readonly IoSHDStatus status;

        private bool disposed;

        #endregion

        #region Constructors

        public EndState(
            IIoStateMachine parentStateMachine,
            IoSHDStatus status,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status;

            logger.LogTrace("1:Method Start");
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
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}");

            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                this.Logger.LogTrace("End State State ProcessMessage Notification Event");
                var endNotification = new FieldNotificationMessage(
                    null,
                    "IO Reset complete",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.IoReset,
                    MessageStatus.OperationEnd);

                this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void ProcessResponseMessage(IoSHDReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Elevator motor on={message.ElevatorMotorOn}");

            //TEMP Check the matching between the status output flags and the message output flags (i.e. the switch ElevatorMotorON has been processed)
            if (this.status.MatchOutputs(message.Outputs))
            {
                this.Logger.LogTrace("End State State ProcessMessage Notification Event");
                var endNotification = new FieldNotificationMessage(
                    null,
                    "IO Reset complete",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.IoReset,
                    MessageStatus.OperationEnd);

                this.Logger.LogTrace($"2:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.ParentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        public override void Start()
        {
            var resetSecurityIoMessage = new IoSHDWriteMessage();

            resetSecurityIoMessage.SwitchElevatorMotor(true);

            this.Logger.LogTrace($"1:Switch elevator MotorON IO={resetSecurityIoMessage}");
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
