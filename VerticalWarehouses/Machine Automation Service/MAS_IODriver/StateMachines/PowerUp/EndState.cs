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
            if (message.ValidOutputs && message.ElevatorMotorOn)
            {
                var endNotification = new NotificationMessage(null, "I/O Powerup complete", MessageActor.Any,
                    MessageActor.IODriver, MessageType.IOPowerUp, MessageStatus.OperationEnd);
                this.parentStateMachine.PublishNotificationEvent(endNotification);
            }
        }

        #endregion
    }
}
