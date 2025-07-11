﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    internal sealed class SwitchAxisEndState : IoStateBase
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly bool hasError;

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public SwitchAxisEndState(
            Axis axisToSwitchOn,
            IoStatus status,
            IoIndex index,
            bool hasError,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.status = status;
            this.axisToSwitchOn = axisToSwitchOn;
            this.index = index;
            this.hasError = hasError;

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
            this.Logger.LogTrace($"1:Switch axis end {this.axisToSwitchOn}");

            var messageData = new SwitchAxisFieldMessageData(this.axisToSwitchOn, MessageVerbosity.Info);
            var endNotification = new FieldNotificationMessage(
                messageData,
                "Motor Switch complete",
                FieldMessageActor.IoDriver,
                FieldMessageActor.IoDriver,
                FieldMessageType.SwitchAxis,
                this.hasError ? MessageStatus.OperationError : MessageStatus.OperationEnd,
                (byte)this.index);

            this.Logger.LogTrace($"1:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

            this.ParentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion
    }
}
