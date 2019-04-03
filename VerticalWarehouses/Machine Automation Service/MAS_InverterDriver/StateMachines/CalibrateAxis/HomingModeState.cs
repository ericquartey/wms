using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.CalibrateAxis
{
    public class HomingModeState : InverterStateBase
    {
        #region Fields

        private const ushort RESET_STATUS_WORD_VALUE = 0x0250;

        private const int SEND_DELAY = 50;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort stopParameterValue;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingModeState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            ushort parameterValue = 0x0000;

            logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    stopParameterValue = 0x8000;
                    break;

                case Axis.Vertical:
                    stopParameterValue = 0x0000;
                    break;
            }

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.SetOperatingModeParam, parameterValue, SEND_DELAY);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("3:Method End");
        }

        #endregion

        #region Destructors

        ~HomingModeState()
        {
            this.Dispose(false);
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
                this.ParentStateMachine.ChangeState(new ErrorState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
            }

            this.logger.LogTrace($"3:InverterParameterId.SetOperatingModeParam={InverterParameterId.SetOperatingModeParam}");

            if (message.IsWriteMessage && message.ParameterId == InverterParameterId.SetOperatingModeParam)
            {
                this.ParentStateMachine.ChangeState(new ShutdownState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }

            this.logger.LogTrace($"3:UShortPayload={message.UShortPayload}:RESET_STATUS_WORD_VALUE={RESET_STATUS_WORD_VALUE}");

            if ((message.UShortPayload & RESET_STATUS_WORD_VALUE) == RESET_STATUS_WORD_VALUE)
            {
                this.ParentStateMachine.ChangeState(new EndState(this.ParentStateMachine, this.axisToCalibrate, this.logger));
                returnValue = true;
            }

            this.logger.LogDebug("4:Method End");

            return returnValue;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.logger.LogDebug("1:Method Start");

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, stopParameterValue);

            this.logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueMessage(inverterMessage);
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
