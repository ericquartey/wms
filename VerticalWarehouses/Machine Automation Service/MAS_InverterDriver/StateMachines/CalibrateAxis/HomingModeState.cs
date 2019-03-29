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

        private const int sendDelay = 50;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        private bool forceStop;

        #endregion

        #region Constructors

        public HomingModeState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.logger.LogDebug("1:Method Start");

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

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                this.logger.LogTrace($"3:Force Stop={this.forceStop}");

                if (this.forceStop)
                {
                    this.parentStateMachine.ChangeState(new EndState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }
            }

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.parentStateMachine.ChangeState(new ShutdownState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }

            this.logger.LogDebug("4:Method End");

            return returnValue;
        }

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
