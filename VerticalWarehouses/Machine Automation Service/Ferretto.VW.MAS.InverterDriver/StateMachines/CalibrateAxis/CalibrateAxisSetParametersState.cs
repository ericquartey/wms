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
        #region Fields

        private const int CREEP_SPEED_CAROUSEL_DEFAULT = 2000;  // [mm/s]

        private const int CREEP_SPEED_ELEVATOR_DEFAULT = 500;   // [mm/s]

        private const int FAST_SPEED_CAROUSEL_DEFAULT = 8000;   // [mm/s]

        private const int FAST_SPEED_ELEVATOR_DEFAULT = 2000;   // [mm/s]

        private const short HORIZONTAL_SENSOR = 548;    // MF2ID

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
                var isOneTonMachine = this.ParentStateMachine.GetRequiredService<IMachineVolatileDataProvider>().IsOneTonMachine.Value;

                this.Logger.LogTrace($"2:message={message}:ID Parameter={message.ParameterId}");

                double speedValue;
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
                                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Horizontal);
                                speedValue = (axisParameters.HomingFastSpeed > 0 && axisParameters.HomingFastSpeed < 10 * FAST_SPEED_ELEVATOR_DEFAULT) ? axisParameters.HomingFastSpeed : FAST_SPEED_ELEVATOR_DEFAULT;
                                fastSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                                    speedValue,
                                    Orientation.Horizontal);
                                if (axisParameters.HomingFastSpeed >= 10 * FAST_SPEED_ELEVATOR_DEFAULT)
                                {
                                    this.Logger.LogWarning($"Homing Fast Speed parameter={axisParameters.HomingFastSpeed} mm/s too high! Limited to {speedValue}, Axis={this.axisToCalibrate}");
                                }
                            }
                            else if (this.axisToCalibrate == Axis.BayChain)
                            {
                                var bayProvider = this.ParentStateMachine.GetRequiredService<IBaysDataProvider>();
                                var bayNumber = bayProvider.GetByInverterIndex(this.InverterStatus.SystemIndex);
                                var bay = bayProvider.GetByNumber(bayNumber);
                                speedValue = (bay.Carousel.HomingFastSpeed > 0 && bay.Carousel.HomingFastSpeed < 10 * FAST_SPEED_CAROUSEL_DEFAULT) ? bay.Carousel.HomingFastSpeed : FAST_SPEED_CAROUSEL_DEFAULT;
                                fastSpeed = (int)(speedValue *
                                    this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                                if (bay.Carousel.HomingFastSpeed >= 10 * FAST_SPEED_CAROUSEL_DEFAULT)
                                {
                                    this.Logger.LogWarning($"Homing Fast Speed parameter={bay.Carousel.HomingFastSpeed} mm/s too high! Limited to {speedValue}, Axis={this.axisToCalibrate}");
                                }
                            }
                            else
                            {
                                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Vertical);
                                speedValue = (axisParameters.HomingFastSpeed > 0 && axisParameters.HomingFastSpeed < 10 * FAST_SPEED_ELEVATOR_DEFAULT) ? axisParameters.HomingFastSpeed : FAST_SPEED_ELEVATOR_DEFAULT;
                                fastSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                                    speedValue,
                                    Orientation.Vertical);
                                if (axisParameters.HomingFastSpeed >= 10 * FAST_SPEED_ELEVATOR_DEFAULT)
                                {
                                    this.Logger.LogWarning($"Homing Fast Speed parameter={axisParameters.HomingFastSpeed} mm/s too high! Limited to {speedValue}, Axis={this.axisToCalibrate}");
                                }
                            }

                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingFastSpeed,
                                fastSpeed);

                            this.Logger.LogDebug($"Set Homing Fast Speed={speedValue} mm/s [{fastSpeed} impulses], Axis={this.axisToCalibrate}");

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
                                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Horizontal);
                                speedValue = (axisParameters.HomingCreepSpeed > 0 && axisParameters.HomingCreepSpeed < 10 * CREEP_SPEED_ELEVATOR_DEFAULT) ? axisParameters.HomingCreepSpeed : CREEP_SPEED_ELEVATOR_DEFAULT;
                                creepSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                                    speedValue,
                                    Orientation.Horizontal);
                                if (axisParameters.HomingCreepSpeed >= 10 * CREEP_SPEED_ELEVATOR_DEFAULT)
                                {
                                    this.Logger.LogWarning($"Homing Creep Speed parameter={axisParameters.HomingCreepSpeed} mm/s too high! Limited to {speedValue}, Axis={this.axisToCalibrate}");
                                }
                            }
                            else if (this.axisToCalibrate == Axis.BayChain)
                            {
                                var bayProvider = this.ParentStateMachine.GetRequiredService<IBaysDataProvider>();
                                var bayNumber = bayProvider.GetByInverterIndex(this.InverterStatus.SystemIndex);
                                var bay = bayProvider.GetByNumber(bayNumber);

                                speedValue = (bay.Carousel.HomingCreepSpeed > 0 && bay.Carousel.HomingCreepSpeed < 10 * CREEP_SPEED_CAROUSEL_DEFAULT) ? bay.Carousel.HomingCreepSpeed : CREEP_SPEED_CAROUSEL_DEFAULT;
                                creepSpeed = (int)(speedValue *
                                    this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                                if (bay.Carousel.HomingCreepSpeed >= 10 * CREEP_SPEED_CAROUSEL_DEFAULT)
                                {
                                    this.Logger.LogWarning($"Homing Creep Speed parameter={bay.Carousel.HomingCreepSpeed} mm/s too high! Limited to {speedValue}, Axis={this.axisToCalibrate}");
                                }
                            }
                            else
                            {
                                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Vertical);
                                speedValue = (axisParameters.HomingCreepSpeed > 0 && axisParameters.HomingCreepSpeed < 10 * CREEP_SPEED_ELEVATOR_DEFAULT) ? axisParameters.HomingCreepSpeed : CREEP_SPEED_ELEVATOR_DEFAULT;
                                creepSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                                    speedValue,
                                    Orientation.Vertical);
                                if (axisParameters.HomingCreepSpeed >= 10 * CREEP_SPEED_ELEVATOR_DEFAULT)
                                {
                                    this.Logger.LogWarning($"Homing Creep Speed parameter={axisParameters.HomingCreepSpeed} mm/s too high! Limited to {speedValue}, Axis={this.axisToCalibrate}");
                                }
                            }

                            var inverterMessage = new InverterMessage(
                                this.InverterStatus.SystemIndex,
                                (short)InverterParameterId.HomingCreepSpeed,
                                creepSpeed);

                            this.Logger.LogDebug($"Set Homing Creep Speed={speedValue} mm/s [{creepSpeed} impulses], Axis={this.axisToCalibrate}");

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
