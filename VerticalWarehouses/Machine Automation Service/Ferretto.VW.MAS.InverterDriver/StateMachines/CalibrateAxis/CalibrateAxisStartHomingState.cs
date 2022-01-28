using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisStartHomingState : InverterStateBase
    {
        #region Fields

        private const int CheckDelayTime = 1000;

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        private readonly IErrorsProvider errorProvider;

        private readonly IInvertersProvider invertersProvider;

        private double startPosition;

        private DateTime startTime;

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
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.invertersProvider = this.ParentStateMachine.GetRequiredService<IInvertersProvider>();
        }

        #endregion

        #region Properties

        public IHomingInverterStatus Inverter { get; }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"Calibrate start homing axis {this.axisToCalibrate}");

            this.startTime = DateTime.UtcNow;

            this.Inverter.HomingControlWord.HomingOperation = true;

            if (this.axisToCalibrate == Axis.Horizontal && this.Inverter is AngInverterStatus angInverter)
            {
                this.startPosition = this.invertersProvider.ConvertPulsesToMillimeters(angInverter.CurrentPositionAxisHorizontal, Orientation.Horizontal);
            }
            else if (this.axisToCalibrate == Axis.Horizontal && this.Inverter is AcuInverterStatus acuInverter)
            {
                this.startPosition = this.invertersProvider.ConvertPulsesToMillimeters(acuInverter.CurrentPosition, Orientation.Horizontal);
            }

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
                new CalibrateAxisDisableOperationState(
                    this.ParentStateMachine,
                    this.axisToCalibrate,
                    this.calibration,
                    this.InverterStatus,
                    this.Logger,
                    true));
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
                var delayElapsed = DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > CheckDelayTime;
                if (this.InverterStatus is AngInverterStatus currentStatus)
                {
                    if (currentStatus.CommonStatusWord.IsFault)
                    {
                        this.Logger.LogError($"Horizontal homing error, Inverter fault");
                        this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                    }
                    if (delayElapsed && currentStatus.HomingStatusWord.HomingAttained)
                    {
                        this.ParentStateMachine.ChangeState(new CalibrateAxisDisableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                        returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                    }
                    else if (delayElapsed && this.axisToCalibrate == Axis.Horizontal)
                    {
                        var position = this.invertersProvider.ConvertPulsesToMillimeters(currentStatus.CurrentPositionAxisHorizontal, Orientation.Horizontal);
                        if (Math.Abs(position) > 50
                            && Math.Abs(this.startPosition - position) > 50)
                        {
                            this.Logger.LogError($"Horizontal homing error, maximum space exceeded: {Math.Abs(this.startPosition - position)}mm");
                            this.errorProvider.RecordNew(MachineErrorCode.ZeroSensorErrorAfterHoming);
                            this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                        }
                    }
                }

                if (this.InverterStatus is AcuInverterStatus currentAcuStatus)
                {
                    if (currentAcuStatus.CommonStatusWord.IsFault)
                    {
                        this.Logger.LogError($"Horizontal homing error, Inverter fault");
                        this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                    }
                    if (delayElapsed && currentAcuStatus.HomingStatusWord.HomingAttained)
                    {
                        this.ParentStateMachine.ChangeState(new CalibrateAxisDisableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                        returnValue = true;     // EvaluateReadMessage will stop sending StatusWordParam
                    }
                    else if (delayElapsed && this.axisToCalibrate == Axis.Horizontal)
                    {
                        var position = this.invertersProvider.ConvertPulsesToMillimeters(currentAcuStatus.CurrentPosition, Orientation.Horizontal);
                        if (Math.Abs(position) > 50
                            && Math.Abs(this.startPosition - position) > 50)
                        {
                            this.Logger.LogError($"Horizontal homing error, maximum space exceeded: {Math.Abs(this.startPosition - position)}mm");
                            this.errorProvider.RecordNew(MachineErrorCode.ZeroSensorErrorAfterHoming);
                            this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                        }
                    }
                }
            }

            return returnValue;
        }

        #endregion
    }
}
