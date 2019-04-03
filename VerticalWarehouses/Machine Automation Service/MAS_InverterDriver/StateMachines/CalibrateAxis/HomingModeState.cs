using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class HomingModeState : InverterStateBase
    {
        #region Fields

        private const ushort RESET_STATUS_WORD_VALUE = 0x0250;

        private const int sendDelay = 50;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public HomingModeState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;

            this.parameterValue = 0x0006;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, this.parameterValue);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            this.logger.LogDebug("1:Method Start");

            var returnValue = false;

            this.logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            this.logger.LogTrace($"3:InverterParameterId.SetOperatingModeParam={InverterParameterId.SetOperatingModeParam}");

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.parentStateMachine.ChangeState(new ShutdownState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }

            this.logger.LogTrace($"3:UShortPayload={message.UShortPayload}:RESET_STATUS_WORD_VALUE={RESET_STATUS_WORD_VALUE}");

            if ((message.UShortPayload & RESET_STATUS_WORD_VALUE) == RESET_STATUS_WORD_VALUE)
            {
                this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
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
