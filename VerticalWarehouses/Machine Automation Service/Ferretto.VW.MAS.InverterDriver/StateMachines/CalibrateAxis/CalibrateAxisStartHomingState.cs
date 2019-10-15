using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal class CalibrateAxisStartHomingState : InverterStateBase
    {
        #region Fields

        private const int CheckDelayTime = 1000;

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        private readonly Timer delayCheckTimer;

        private bool delayElapsed;

        #endregion

        #region Constructors

        public CalibrateAxisStartHomingState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            Calibration calibration,
            IHomingInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.calibration = calibration;
            this.Inverter = inverterStatus;
            this.delayCheckTimer = new Timer(this.DelayCheck, null, -1, Timeout.Infinite);
        }

        #endregion

        #region Properties

        public IHomingInverterStatus Inverter { get; }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Calibrate start homing axis {this.axisToCalibrate}");
            this.delayCheckTimer.Change(CheckDelayTime, CheckDelayTime);
            this.delayElapsed = false;

            this.Inverter.HomingControlWord.HomingOperation = true;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWord,
                    this.Inverter.HomingControlWord.Value));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Calibrate Stop requested");

            this.ParentStateMachine.ChangeState(
                new CalibrateAxisStopState(
                    this.ParentStateMachine,
                    this.axisToCalibrate,
                    this.calibration,
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;    // EvaluateWriteMessage will send a StatusWordParam
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;    // EvaluateReadMessage will send a new StatusWordParam after receiving a StatusWord response

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus is AngInverterStatus currentStatus)
                {
                    if (this.delayElapsed && currentStatus.HomingStatusWord.HomingAttained)
                    {
                        this.ParentStateMachine.ChangeState(new CalibrateAxisDisableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                        returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                    }
                }

                if (this.InverterStatus is AcuInverterStatus currentAcuStatus)
                {
                    if (this.delayElapsed && currentAcuStatus.HomingStatusWord.HomingAttained)
                    {
                        this.ParentStateMachine.ChangeState(new CalibrateAxisDisableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                        returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                    }
                }
            }

            return returnValue;
        }

        protected override void OnDisposing()
        {
            this.delayCheckTimer?.Dispose();
        }

        private void DelayCheck(object state)
        {
            // stop timer
            this.delayCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // delay expired
            this.delayElapsed = true;
        }

        #endregion
    }
}
