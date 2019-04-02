using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
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

            this.logger.LogTrace(string.Format("2:{0}", resetSecurityIoMessage));

            resetSecurityIoMessage.SwitchElevatorMotor(true);

            parentStateMachine.EnqueueMessage(resetSecurityIoMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                var endNotification = new NotificationMessage(null, "I/O Powerup complete", MessageActor.Any,
                    MessageActor.IODriver, MessageType.IOPowerUp, MessageStatus.OperationEnd);

                this.logger.LogTrace(string.Format("2:{0}:{1}:{2}",
                    endNotification.Type,
                    endNotification.Destination,
                    endNotification.Status));

                this.parentStateMachine.PublishNotificationEvent(endNotification);
            }

            this.logger.LogDebug("3:End Start");
        }

        #endregion
    }
}
