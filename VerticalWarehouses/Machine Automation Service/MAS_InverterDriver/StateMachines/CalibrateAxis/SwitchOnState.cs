using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class SwitchOnState : InverterStateBase
    {
        #region Fields

        private const int sendDelay = 50;

        private const ushort StatusWordValue = 0x0033;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        private bool forceStop;

        #endregion

        #region Constructors

        public SwitchOnState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.logger.LogDebug("1:Method Start");

            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;
            this.forceStop = false;

            this.logger.LogTrace($"2:Axis to calibrate={this.axisToCalibrate}");

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x8007;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x0007;
                    break;
            }

            var inverterMessage =
                new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, sendDelay);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);
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
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:Force Stop={this.forceStop}:UShortPayload={message.UShortPayload}:StatusWordValue={StatusWordValue}");

                if (this.forceStop)
                {
                    this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }

                if ((message.UShortPayload & StatusWordValue) == StatusWordValue)
                {
                    this.parentStateMachine.ChangeState(new EnableOperationState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }
            }

            this.logger.LogDebug("4:Method End");

            return returnValue;
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            this.forceStop = true;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, sendDelay);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion
    }
}
