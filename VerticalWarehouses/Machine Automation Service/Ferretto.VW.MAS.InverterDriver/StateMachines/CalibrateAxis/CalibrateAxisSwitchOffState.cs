using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal class CalibrateAxisSwitchOffState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        #endregion

        #region Constructors

        public CalibrateAxisSwitchOffState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug($"Calibrate Switch Off axis {this.axisToCalibrate}");
            if (this.InverterStatus is IHomingInverterStatus currentStatus)
            {
                currentStatus.HomingControlWord.SwitchOn = false;

                this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWordParam,
                    currentStatus.HomingControlWord.Value));
            }
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Stop process already active");
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (!this.InverterStatus.CommonStatusWord.IsSwitchedOn)
                {
                    this.ParentStateMachine.ChangeState(
                        new CalibrateAxisEndState(
                            this.ParentStateMachine,
                            this.axisToCalibrate,
                            this.InverterStatus,
                            this.Logger,
                            true));

                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
