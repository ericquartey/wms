using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisSetParametersState : InverterStateBase
    {
        //private const int HORIZONTAL_OFFSET = -500;

        //private const int HORIZONTAL_OFFSET_ONETON_MACHINE = -800;

        #region Fields

        // TODO move following parameters into configuration? si
        private const int HIGH_SPEED = 2000;

        private const int HIGH_SPEED_BAY = 8000;

        private const short HORIZONTAL_SENSOR = 548;    // MF2ID

        private const int LOW_SPEED = 500;

        private const int LOW_SPEED_BAY = 2000;

        private const short VERTICAL_SENSOR = 527;      // S3IND

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
            InverterCalibrationMode calibrationMode;
            if (this.axisToCalibrate == Axis.Vertical)
            {
                calibrationMode = InverterCalibrationMode.Elevator;
            }
            else if (this.calibration == Calibration.FindSensor)
            {
                if (this.axisToCalibrate == Axis.BayChain)
                {
                    calibrationMode = InverterCalibrationMode.FindSensorCarousel;
                }
                else
                {
                    calibrationMode = InverterCalibrationMode.FindSensor;
                }
            }
            else
            {
                calibrationMode = InverterCalibrationMode.ResetEncoder;
            }
            this.Logger.LogDebug($"1:Calibrate Set Parameters, Axis={this.axisToCalibrate}, calibration={calibrationMode}");

            var inverterMessage = new InverterMessage(
                this.InverterStatus.SystemIndex,
                (short)InverterParameterId.HomingCalibration,
                (ushort)calibrationMode,
                (this.axisToCalibrate == Axis.Vertical ? InverterDataset.HomingCalibrationElevator : InverterDataset.HomingCalibration));

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
            var returnValue = false;
            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
            }
            else
            {
                var isOneTonMachine = this.ParentStateMachine.GetRequiredService<IMachineProvider>().IsOneTonMachine();

                this.Logger.LogTrace($"2:message={message}:ID Parameter={message.ParameterId}");

                int speedValueToLog;
                switch (message.ParameterId)
                {
                    case InverterParameterId.HomingCalibration:
                        {
                            var sensor = (this.axisToCalibrate == Axis.Vertical || isOneTonMachine) ? VERTICAL_SENSOR : HORIZONTAL_SENSOR;
                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingSensor,
                                sensor);

                            this.Logger.LogDebug($"Set Homing Sensor={sensor}, Axis={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);

                            break;
                        }

                    case InverterParameterId.HomingSensor:
                        {
                            int fastSpeed;
                            if (this.axisToCalibrate == Axis.Horizontal)
                            {
                                speedValueToLog = HIGH_SPEED;
                                fastSpeed = HIGH_SPEED;
                                //fastSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(HIGH_SPEED, Orientation.Horizontal);
                            }
                            else if (this.axisToCalibrate == Axis.BayChain)
                            {
                                speedValueToLog = HIGH_SPEED_BAY;
                                fastSpeed = (int)(HIGH_SPEED_BAY * this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                            }
                            else
                            {
                                speedValueToLog = HIGH_SPEED;
                                fastSpeed = HIGH_SPEED;
                                //fastSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(HIGH_SPEED, Orientation.Vertical);
                            }

                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingFastSpeed,
                                fastSpeed);

                            this.Logger.LogDebug($"Set Homing Fast Speed={speedValueToLog} mm/s [{fastSpeed} impulses], Axis={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            break;
                        }

                    case InverterParameterId.HomingFastSpeed:
                        {
                            int offset;
                            if (this.axisToCalibrate == Axis.Horizontal)
                            {
                                if (this.calibration == Calibration.FindSensor)
                                {
                                    var axis = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Horizontal);

                                    offset = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(axis.ChainOffset, Orientation.Horizontal);
                                }
                                else
                                {
                                    offset = 0;
                                }
                            }
                            else if (this.axisToCalibrate == Axis.BayChain)
                            {
                                offset = (int)(this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetChainOffset(this.InverterStatus.SystemIndex)
                                    * this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                            }
                            else
                            {
                                offset = 0;
                            }

                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingOffset,
                                offset);

                            this.Logger.LogDebug($"Set Homing offset={offset}, Axis={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            break;
                        }

                    case InverterParameterId.HomingOffset:
                        {
                            int creepSpeed;
                            if (this.axisToCalibrate == Axis.Horizontal)
                            {
                                speedValueToLog = LOW_SPEED;
                                creepSpeed = LOW_SPEED;
                                //creepSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(LOW_SPEED, Orientation.Horizontal);
                            }
                            else if (this.axisToCalibrate == Axis.BayChain)
                            {
                                speedValueToLog = LOW_SPEED_BAY;
                                creepSpeed = (int)(LOW_SPEED_BAY * this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                            }
                            else
                            {
                                speedValueToLog = LOW_SPEED;
                                creepSpeed = LOW_SPEED;
                                //creepSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(LOW_SPEED, Orientation.Vertical);
                            }

                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingCreepSpeed,
                                creepSpeed);

                            this.Logger.LogDebug($"Set Homing Creep Speed={speedValueToLog} mm/s [{creepSpeed} impulses], Axis={this.axisToCalibrate}");

                            this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            break;
                        }
                    case InverterParameterId.HomingCreepSpeed:
                        this.ParentStateMachine.ChangeState(new CalibrateAxisEnableOperationState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
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
