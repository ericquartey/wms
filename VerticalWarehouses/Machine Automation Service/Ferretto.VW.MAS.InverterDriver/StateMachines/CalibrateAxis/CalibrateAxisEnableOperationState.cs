﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal class CalibrateAxisEnableOperationState : InverterStateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

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
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug($"1:Axis to calibrate={this.axisToCalibrate}");

            this.InverterStatus.CommonControlWord.HorizontalAxis = (this.axisToCalibrate == Axis.Horizontal);

            this.InverterStatus.CommonControlWord.EnableOperation = true;

            var inverterMessage = new InverterMessage(
                this.InverterStatus.SystemIndex,
                (short)InverterParameterId.ControlWordParam,
                this.InverterStatus.CommonControlWord.Value);

            this.Logger.LogTrace($"2:inverterMessage={inverterMessage}");

            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
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

            return returnValue;
        }

        #endregion
    }
}
