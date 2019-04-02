using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.Reset
{
    public class EndState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public EndState(IIoStateMachine parentStateMachine, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.parentStateMachine = parentStateMachine;
            var resetSecurityIoMessage = new IoMessage(false);

            this.logger.LogTrace($"2:Reset Security IO={resetSecurityIoMessage}");

            resetSecurityIoMessage.SwitchElevatorMotor(true);
            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

            this.logger.LogDebug("3:Method End");
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
                var endNotification = new NotificationMessage(null, "IO Reset complete", MessageActor.Any,
                    MessageActor.IODriver, MessageType.IOReset, MessageStatus.OperationEnd);

                this.logger.LogTrace($"3:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                this.parentStateMachine.PublishNotificationEvent(endNotification);
            }

            this.logger.LogDebug("4:Method End");
        }

        #endregion
    }
}
