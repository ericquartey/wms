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

        private readonly Axis axisToCalibrate;

        private bool homingReachedReset;

        #endregion

        #region Constructors

        public CalibrateAxisStartHomingState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            IHomingInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;

            this.Inverter = inverterStatus;
        }

        #endregion

        #region Properties

        public IHomingInverterStatus Inverter { get; }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Calibrate start homing axis {this.axisToCalibrate}");
            this.Inverter.HomingControlWord.HomingOperation = true;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.ControlWordParam,
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
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
                if (this.InverterStatus is AngInverterStatus currentStatus)
                {
                    if (this.axisToCalibrate == Axis.Horizontal)
                    {
                        this.homingReachedReset = true;
                    }
                    if (!currentStatus.HomingStatusWord.HomingAttained)
                    {
                        this.homingReachedReset = true;
                    }
                    if (this.homingReachedReset && currentStatus.HomingStatusWord.HomingAttained)
                    {
                        this.ParentStateMachine.ChangeState(new CalibrateAxisDisableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
                        returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                    }
                }

                if (this.InverterStatus is AcuInverterStatus currentAcuStatus)
                {
                    if (this.axisToCalibrate == Axis.Horizontal)
                    {
                        this.homingReachedReset = true;
                    }
                    if (!currentAcuStatus.HomingStatusWord.HomingAttained)
                    {
                        this.homingReachedReset = true;
                    }
                    if (this.homingReachedReset && currentAcuStatus.HomingStatusWord.HomingAttained)
                    {
                        this.ParentStateMachine.ChangeState(new CalibrateAxisDisableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
                        returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                    }
                }
            }

            return returnValue;
        }

        #endregion
    }
}
