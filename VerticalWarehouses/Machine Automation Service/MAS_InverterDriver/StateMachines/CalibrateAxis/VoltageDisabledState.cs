using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class VoltageDisabledState : InverterStateBase
    {
        #region Fields

        private const ushort RESET_STATUS_WORD_VALUE = 0x0250;

        private const int sendDelay = 50;

        private const ushort STATUS_WORD_VALUE = 0x0050;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public VoltageDisabledState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;

            this.logger.LogTrace($"2:Axis to calibrate={this.axisToCalibrate}");

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x8000;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x0000;
                    break;
            }

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, sendDelay);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            var messageData = new CalibrateAxisMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                messageData,
                $"{this.axisToCalibrate} Homing started",
                MessageActor.Any,
                MessageActor.InverterDriver,
                MessageType.CalibrateAxis,
                MessageStatus.OperationStart,
                ErrorLevel.NoError,
                MessageVerbosity.Info);

            this.logger.LogTrace($"4:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.parentStateMachine.PublishNotificationEvent(notificationMessage);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");
            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}:InverterParameterId.StatusWordParam{InverterParameterId.StatusWordParam}");

            var returnValue = false;

            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload}:STATUS_WORD_VALUE={STATUS_WORD_VALUE}:RESET_STATUS_WORD_VALUE={RESET_STATUS_WORD_VALUE}");

                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
                {
                    this.parentStateMachine.ChangeState(new HomingModeState(this.parentStateMachine, this.axisToCalibrate, this.logger));

                    return true;
                }

                if ((message.UShortPayload & RESET_STATUS_WORD_VALUE) == RESET_STATUS_WORD_VALUE)
                {
                    this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }
            }

            this.logger.LogDebug("4:Method End");

            return returnValue;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            ushort value = 0x0000;
            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    value = 0x8000;
                    break;

                case Axis.Vertical:
                    value = 0x0000;
                    break;
            }

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, value, sendDelay);
            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");
            this.parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion
    }
}
