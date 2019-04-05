using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class VoltageDisabledState : InverterStateBase
    {
        #region Fields

        private const int SEND_DELAY = 50;

        private const ushort STATUS_WORD_VALUE = 0x0050;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        private bool disposed;

        #endregion

        #region Constructors

        public VoltageDisabledState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");
            this.logger = logger;

            this.ParentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;

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

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, SEND_DELAY);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            var messageData = new CalibrateAxisFieldMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new FieldNotificationMessage(
                messageData,
                $"{this.axisToCalibrate} Homing started",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.CalibrateAxis,
                MessageStatus.OperationStart);

            this.logger.LogTrace($"4:Type={notificationMessage.Type}:Destination={notificationMessage.Destination}:Status={notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            this.logger.LogDebug("5:Method End");
        }

        #endregion

        #region Destructors

        ~VoltageDisabledState()
        {
            this.Dispose(false);
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
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:UShortPayload={message.UShortPayload}:STATUS_WORD_VALUE={STATUS_WORD_VALUE}");

                if ((message.UShortPayload & STATUS_WORD_VALUE) == STATUS_WORD_VALUE)
                {
                    this.ParentStateMachine.ChangeState(new HomingModeState(this.ParentStateMachine, this.axisToCalibrate, this.logger));

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

            this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.axisToCalibrate, this.logger, true));

            this.logger.LogDebug("2:Method End");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
