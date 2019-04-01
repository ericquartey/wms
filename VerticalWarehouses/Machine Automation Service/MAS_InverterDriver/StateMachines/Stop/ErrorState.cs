using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.Stop
{
    public class ErrorState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ErrorState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            this.axisToStop = axisToCalibrate;

            var messageData = new StopAxisMessageData(this.axisToStop);

            var errorNotification = new NotificationMessage(messageData, "Inverter operation error", MessageActor.Any,
                MessageActor.InverterDriver, MessageType.Stop, MessageStatus.OperationError, ErrorLevel.Error);

            this.logger.LogTrace($"2:Type={errorNotification.Type}:Destination={errorNotification.Destination}:Status={errorNotification.Status}");

            parentStateMachine.PublishNotificationEvent(errorNotification);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            return false;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
