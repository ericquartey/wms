using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal class CalibrateAxisSetParametersState : InverterStateBase
    {
        #region Fields

        // TODO move following parameters into configuration
        private const int HIGH_SPEED = 2000;

        private const short HORIZONTAL_SENSOR = 1807;

        private const int LOW_SPEED = 500;

        private const short VERTICAL_SENSOR = 527;

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        #endregion

        #region Constructors

        public CalibrateAxisSetParametersState(
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
            this.Logger.LogDebug($"1:Calibrate Set Parameters, Axis ={this.axisToCalibrate}, calibration ={this.calibration}");

            var inverterMessage = new InverterMessage(
                this.InverterStatus.SystemIndex,
                (short)InverterParameterId.HomingCalibration,
                (ushort)(this.calibration == Calibration.FindSensor ? InverterCalibrationMode.FindSensor : InverterCalibrationMode.ResetEncoder),
                InverterDataset.HomingCalibration);

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
                    this.InverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            var returnValue = false;
            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:ID Parameter={message.ParameterId}");
                switch (message.ParameterId)
                {
                    case InverterParameterId.HomingCalibration:
                        {
                            var sensor = (this.axisToCalibrate == Axis.Vertical || this.ParentStateMachine.GetRequiredService<IMachineProvider>().IsOneTonMachine() ? VERTICAL_SENSOR : HORIZONTAL_SENSOR);
                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingSensor,
                                sensor);

                            this.Logger.LogDebug($"Set Homing Sensor={sensor}, Axis ={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                            break;
                        }

                    case InverterParameterId.HomingSensor:
                        {
                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingFastSpeedParam,
                                HIGH_SPEED);

                            this.Logger.LogDebug($"Set Homing Fast Speed={HIGH_SPEED}, Axis ={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            break;
                        }

                    case InverterParameterId.HomingFastSpeedParam:
                        {
                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingCreepSpeedParam,
                                LOW_SPEED);

                            this.Logger.LogDebug($"Set Homing Low Speed={LOW_SPEED}, Axis ={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            break;
                        }
                    case InverterParameterId.HomingCreepSpeedParam:
                        this.ParentStateMachine.ChangeState(new CalibrateAxisEnableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.InverterStatus, this.Logger));
                        break;
                }
            }
            return returnValue;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return false;
        }

        #endregion
    }
}
