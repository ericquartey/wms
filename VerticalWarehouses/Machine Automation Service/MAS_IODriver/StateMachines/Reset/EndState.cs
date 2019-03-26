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
            logger.LogInformation("End State State CTor");
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;

            var resetSecurityIoMessage = new IoMessage(false);
            resetSecurityIoMessage.SwitchElevatorMotor(true);

            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            logger.LogTrace("End State ProcessMessage");
            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                logger.LogTrace("End State State ProcessMessage Notification Event");
                var endNotification = new NotificationMessage(null, "IO Reset complete", MessageActor.Any,
                    MessageActor.IODriver, MessageType.IOReset, MessageStatus.OperationEnd);
                this.parentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        #endregion
    }
}
