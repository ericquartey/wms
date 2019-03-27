﻿using Ferretto.VW.Common_Utils.Enumerations;
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
            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");

            var messageData = new SwitchAxisMessageData(axisToSwitchOn);
            var endNotification = new NotificationMessage(messageData, "Motor Switch complete", MessageActor.Any,
                MessageActor.IODriver, MessageType.SwitchAxis, MessageStatus.OperationEnd);
            this.logger.LogTrace($"2-Constructor: published notification: {endNotification.Type}, {endNotification.Status}, {endNotification.Destination}");
            this.parentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            this.logger.LogTrace($"1-Message processed: {message.Inputs?.ToString()}, {message.Outputs?.ToString()}");
        }

        #endregion
    }
}
