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
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.logger.LogTrace($"1-Constructor");

            var messageData = new CalibrateAxisMessageData(this.axisToCalibrate);

            var errorNotification = new NotificationMessage(messageData, "Inverter operation error", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.CalibrateAxis, MessageStatus.OperationError, ErrorLevel.Error);
            this.logger.LogTrace($"2-Constructor: published notification: {errorNotification.Type}, {errorNotification.Status}, {errorNotification.Destination}");
            parentStateMachine.PublishNotificationEvent(errorNotification);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogTrace($"Message processed: {message.ParameterId}, {message.Payload}");
            return false;
        }

        /// <inheritdoc />
        public override void Stop()
        {
        }

        #endregion
    }
}
