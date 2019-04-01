using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class EndState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public EndState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");

            var messageData = new CalibrateAxisMessageData(axisToCalibrate);
            var endNotification = new NotificationMessage(messageData, "Axis calibration complete", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.CalibrateAxis, MessageStatus.OperationEnd);
            this.logger.LogTrace($"2-Constructor: published notification: {endNotification.Type}, {endNotification.Status}, {endNotification.Destination}");
            this.parentStateMachine.PublishNotificationEvent(endNotification);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogTrace($"1-Message processed: {message.ParameterId}, {message.Payload}");

            if (message.IsError)
            {
                this.logger.LogTrace($"2- Change State to ErrorState");
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            return true;
        }

        /// <inheritdoc />
        public override void Stop()
        {
        }

        #endregion
    }
}
