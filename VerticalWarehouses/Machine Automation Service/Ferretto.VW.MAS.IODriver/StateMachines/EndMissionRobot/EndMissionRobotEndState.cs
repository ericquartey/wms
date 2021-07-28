using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.EndMissionRobot
{
    internal sealed class EndMissionRobotEndState : IoStateBase
    {
        #region Fields

        private readonly bool enable;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public EndMissionRobotEndState(
            bool enable,
            IoStatus status,
            ILogger logger,
            IIoStateMachine parentStateMachine,
            IoIndex index)
            : base(parentStateMachine, logger)
        {
            this.enable = enable;
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Message processed: {message}");
        }

        public override void Start()
        {
            var endNotification = new FieldNotificationMessage(
                new EndMissionRobotFieldMessageData(this.enable),
                $"Bay light={this.enable} completed",
                FieldMessageActor.IoDriver,
                FieldMessageActor.IoDriver,
                FieldMessageType.EndMissionRobot,
                MessageStatus.OperationEnd,
                (byte)this.index);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
