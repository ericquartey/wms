using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.Stop
{
    public class StopState : InverterStateBase
    {
        #region Fields

        private const ushort StatusWordValue = 0x0050;

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public StopState(IInverterStateMachine parentStateMachine, Axis axisToStop, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.parentStateMachine = parentStateMachine;
            this.logger = logger;
            this.axisToStop = axisToStop;

            this.logger.LogDebug($"2:Axis to stop{this.axisToStop}");

            switch (this.axisToStop)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x8000;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x0000;
                    break;
            }
            var stopMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue);

            this.logger.LogTrace($"3:Stop message={stopMessage}");

            parentStateMachine.EnqueueMessage(stopMessage);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            var returnValue = false;

            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToStop, this.logger));
            }
            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload}:StatusWordValue={StatusWordValue}");

                if ((message.UShortPayload & StatusWordValue) == StatusWordValue)
                {
                    var messageData = new StopAxisMessageData(this.axisToStop);
                    var endNotification = new NotificationMessage(messageData, "Axis calibration complete", MessageActor.Any,
                        MessageActor.InverterDriver, MessageType.InverterReset, MessageStatus.OperationEnd);

                    this.logger.LogTrace($"4:Type={endNotification.Type}:Destination={endNotification.Destination}:Status={endNotification.Status}");

                    this.parentStateMachine.PublishNotificationEvent(endNotification);
                    returnValue = true;
                }
            }

            this.logger.LogDebug("5:Method End");

            return returnValue;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
