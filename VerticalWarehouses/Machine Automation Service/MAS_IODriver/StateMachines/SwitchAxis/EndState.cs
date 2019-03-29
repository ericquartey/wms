using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class EndState : IoStateBase
    {
        #region Fields

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public EndState(Axis axisToSwitchOn, ILogger logger, IIoStateMachine parentStateMachine)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.parentStateMachine = parentStateMachine;

            var messageData = new SwitchAxisMessageData(axisToSwitchOn);
            var endNotification = new NotificationMessage(messageData, "Motor Switch complete", MessageActor.Any,
                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);

            this.logger.LogTrace(string.Format("2:{0}:{1}:{2}",
                endNotification.Type,
                endNotification.Destination,
                endNotification.Status));

            this.parentStateMachine.PublishNotificationEvent(endNotification);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogTrace($"EndState processMessage");
        }

        #endregion
    }
}
