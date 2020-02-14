using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisDisableOperationState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        private bool stopRequested;

        #endregion

        #region Constructors

        public CalibrateAxisDisableOperationState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            Calibration calibration,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            bool stopRequested = false)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.calibration = calibration;
            this.stopRequested = stopRequested;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            if (this.InverterStatus is IHomingInverterStatus currentStatus)
            {
                this.Logger.LogDebug($"Calibrate Disable Operation axis {this.axisToCalibrate}. StopRequested = {this.stopRequested}");
                currentStatus.HomingControlWord.HomingOperation = false;

                var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, currentStatus.HomingControlWord.Value);

                this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
            }
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Calibrate Axis Stop requested");
            this.stopRequested = true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;    // EvaluateWriteMessage will send a StatusWordParam
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus.CommonStatusWord.IsOperationEnabled)
                {
                    if (this.InverterStatus is IHomingInverterStatus currentStatus)
                    {
                        currentStatus.HomingControlWord.EnableOperation = false;
                        var inverterMessage = new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.ControlWord, currentStatus.HomingControlWord.Value);

                        this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");

                        this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                    }
                }
                else
                {
                    this.ParentStateMachine.ChangeState(new CalibrateAxisEndState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger, this.stopRequested));
                    returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                }
            }
            return returnValue;
        }

        #endregion
    }
}
