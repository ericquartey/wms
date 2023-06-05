using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisEnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        private readonly IErrorsProvider errorProvider;

        private readonly int inverterResponseTimeout;

        private readonly IMachineProvider machineProvider;

        private int CheckDelayTime = 300;

        private DateTime enableTime;

        private DateTime startTime;

        #endregion

        #region Constructors

        public CalibrateAxisEnableOperationState(
            IInverterStateMachine parentStateMachine,
            Axis axisToCalibrate,
            Calibration calibration,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.calibration = calibration;
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();
            this.machineProvider = this.ParentStateMachine.GetRequiredService<IMachineProvider>();

            this.inverterResponseTimeout = this.machineProvider.GetInverterResponseTimeout();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"1:Axis to calibrate={this.axisToCalibrate}");

            this.startTime = DateTime.UtcNow;
            this.enableTime = DateTime.MinValue;
            if (this.axisToCalibrate == Axis.Vertical)
            {
                this.CheckDelayTime *= 4;
            }

            this.InverterStatus.CommonControlWord.HorizontalAxis = (this.axisToCalibrate == Axis.Horizontal);

            this.InverterStatus.CommonControlWord.EnableOperation = true;

            var inverterMessage = new InverterMessage(
                this.InverterStatus.SystemIndex,
                (short)InverterParameterId.ControlWord,
                this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
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
                    if (this.enableTime == DateTime.MinValue)
                    {
                        this.enableTime = DateTime.UtcNow;
                    }
                    else if (DateTime.UtcNow.Subtract(this.enableTime).TotalMilliseconds > this.CheckDelayTime)
                    {
                        this.ParentStateMachine.ChangeState(
                            new CalibrateAxisStartHomingState(
                                this.ParentStateMachine,
                                this.axisToCalibrate,
                                this.calibration,
                                this.InverterStatus as IHomingInverterStatus,
                                this.Logger));

                        returnValue = true; // EvaluateReadMessage will stop sending StatusWordParam
                    }
                }
                else if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.inverterResponseTimeout)
                {
                    this.Logger.LogError($"2:CalibrateAxisEnableOperationState timeout, inverter {this.InverterStatus.SystemIndex}");
                    this.errorProvider.RecordNew(MachineErrorCode.InverterCommandTimeout, additionalText: $"Calibrate Axis Enable Operation Inverter {this.InverterStatus.SystemIndex}");
                    this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                }
            }

            return returnValue;
        }

        #endregion
    }
}
