using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisSetParametersState : InverterStateBase
    {
        #region Fields

        // Creep speed default values
        private const int CREEP_SPEED_CAROUSEL_DEFAULT = 5;        // [mm/s]

        private const int CREEP_SPEED_ELEVATOR_HORIZ_DEFAULT = 6;

        private const int CREEP_SPEED_ELEVATOR_VERT_DEFAULT = 6;

        private const int CREEP_SPEED_EXTERNALBAY_DEFAULT = 5;     // [mm/s]

        // [mm/s]

        // [mm/s]

        // Fast speed default values
        private const int FAST_SPEED_CAROUSEL_DEFAULT = 22;        // [mm/s]

        private const int FAST_SPEED_ELEVATOR_HORIZ_DEFAULT = 25;

        private const int FAST_SPEED_ELEVATOR_VERT_DEFAULT = 25;

        private const int FAST_SPEED_EXTERNALBAY_DEFAULT = 22;     // [mm/s]

        // [mm/s]

        // [mm/s]

        private const short HORIZONTAL_SENSOR = 548;    // MF2ID - elevator chain zero sensor

        private const short VERTICAL_SENSOR = 527;      // S3IND - elevator vertical zero position and bay chain zero position

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

                var isCommandToSend = true;
                double speedValue;
                switch (message.ParameterId)
                {
                    case InverterParameterId.HomingCalibration:
                        {
                            if (this.axisToCalibrate == Axis.BayChain)
                            {
                                this.SetFastSpeed();
                            }
                            else
                            {
                                var sensor = (this.axisToCalibrate == Axis.Horizontal && this.InverterStatus.SystemIndex == InverterIndex.MainInverter) ? HORIZONTAL_SENSOR : VERTICAL_SENSOR;
                                var inverterMessage = new InverterMessage(
                                    this.InverterStatus.SystemIndex,
                                    (short)InverterParameterId.HomingSensor,
                                    sensor);

                                this.Logger.LogDebug($"Set Homing Sensor={sensor}, Axis={this.axisToCalibrate}");

                                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            }
                            break;
                        }

                    case InverterParameterId.HomingSensor:
                        {
                            this.SetFastSpeed();

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

                                if (axisParameters.HomingCreepSpeed >= 2 * CREEP_SPEED_ELEVATOR_HORIZ_DEFAULT)
                                {
                                    isCommandToSend = false;
                                    this.Logger.LogError($"Homing Creep Speed parameter={axisParameters.HomingCreepSpeed} mm/s is too high! for Axis={this.axisToCalibrate}");
                                    this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                                }

                                speedValue = axisParameters.HomingCreepSpeed;
                                creepSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                                    speedValue,
                                    Orientation.Horizontal);
                            }
                            else if (this.axisToCalibrate == Axis.BayChain)
                            {
                                var bayProvider = this.ParentStateMachine.GetRequiredService<IBaysDataProvider>();
                                var bayNumber = bayProvider.GetByInverterIndex(this.InverterStatus.SystemIndex);
                                var bay = bayProvider.GetByNumber(bayNumber);

                                if (!bay.IsExternal && bay.Carousel != null)
                                {
                                    // Handle the carousel
                                    if (bay.Carousel.HomingCreepSpeed >= 2 * CREEP_SPEED_CAROUSEL_DEFAULT)
                                    {
                                        isCommandToSend = false;
                                        this.Logger.LogError($"Homing Creep Speed parameter={bay.Carousel.HomingCreepSpeed} mm/s is too high! for Bay={bayNumber}");
                                        this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                                    }

                                    speedValue = bay.Carousel.HomingCreepSpeed;
                                    creepSpeed = (int)(speedValue *
                                        this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                                }
                                else
                                {
                                    // Handle the external bay
                                    if (bay.External.HomingCreepSpeed >= 2 * CREEP_SPEED_EXTERNALBAY_DEFAULT)
                                    {
                                        isCommandToSend = false;
                                        this.Logger.LogError($"Homing Creep Speed parameter={bay.External.HomingCreepSpeed} mm/s is too high! for Bay={bayNumber}");
                                        this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                                    }

                                    speedValue = bay.External.HomingCreepSpeed;
                                    creepSpeed = (int)(speedValue *
                                        this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                                }
                            }
                            else
                            {
                                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Vertical);

                                if (axisParameters.HomingCreepSpeed >= 2 * CREEP_SPEED_ELEVATOR_VERT_DEFAULT)
                                {
                                    isCommandToSend = false;
                                    this.Logger.LogError($"Homing Creep Speed parameter={axisParameters.HomingCreepSpeed} mm/s is too high! for Axis={this.axisToCalibrate}");
                                    this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                                }

                                speedValue = axisParameters.HomingCreepSpeed;
                                creepSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                                    speedValue,
                                    Orientation.Vertical);
                            }

                            if (isCommandToSend)
                            {
                                var inverterMessage = new InverterMessage(
                                    this.InverterStatus.SystemIndex,
                                    (short)InverterParameterId.HomingCreepSpeed,
                                    creepSpeed);

                                this.Logger.LogDebug($"Set Homing Creep Speed={speedValue} mm/s [{creepSpeed} impulses], Axis={this.axisToCalibrate}");

                                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
                            }

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

        private void SetFastSpeed()
        {
            double speedValue;
            var isCommandToSend = true;
            int fastSpeed;
            if (this.axisToCalibrate == Axis.Horizontal)
            {
                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Horizontal);

                if (axisParameters.HomingFastSpeed >= 2 * FAST_SPEED_ELEVATOR_HORIZ_DEFAULT)
                {
                    isCommandToSend = false;
                    this.Logger.LogError($"Homing Fast Speed parameter={axisParameters.HomingFastSpeed} mm/s is too high! for Axis={this.axisToCalibrate}");
                    this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                }

                speedValue = axisParameters.HomingFastSpeed;
                fastSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                    speedValue,
                    Orientation.Horizontal);
            }
            else if (this.axisToCalibrate == Axis.BayChain)
            {
                var bayProvider = this.ParentStateMachine.GetRequiredService<IBaysDataProvider>();
                var bayNumber = bayProvider.GetByInverterIndex(this.InverterStatus.SystemIndex);
                var bay = bayProvider.GetByNumber(bayNumber);

                if (!bay.IsExternal && bay.Carousel != null)
                {
                    // Handle the carousel
                    if (bay.Carousel.HomingFastSpeed >= 2 * FAST_SPEED_CAROUSEL_DEFAULT)
                    {
                        isCommandToSend = false;
                        this.Logger.LogError($"Homing Fast Speed parameter={bay.Carousel.HomingFastSpeed} mm/s is too high! for Bay={bayNumber}");
                        this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                    }

                    speedValue = bay.Carousel.HomingFastSpeed;
                    fastSpeed = (int)(speedValue *
                        this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                }
                else
                {
                    // Handle the external bay
                    if (bay.External.HomingFastSpeed >= 2 * FAST_SPEED_EXTERNALBAY_DEFAULT)
                    {
                        isCommandToSend = false;
                        this.Logger.LogError($"Homing Fast Speed parameter={bay.External.HomingFastSpeed} mm/s is too high! for Bay={bayNumber}");
                        this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                    }

                    speedValue = bay.External.HomingFastSpeed;
                    fastSpeed = (int)(speedValue *
                        this.ParentStateMachine.GetRequiredService<IBaysDataProvider>().GetResolution(this.InverterStatus.SystemIndex));
                }
            }
            else
            {
                var axisParameters = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Vertical);

                if (axisParameters.HomingFastSpeed >= 2 * FAST_SPEED_ELEVATOR_VERT_DEFAULT)
                {
                    isCommandToSend = false;
                    this.Logger.LogError($"Homing Fast Speed parameter={axisParameters.HomingFastSpeed} mm/s is too high! for Axis={this.axisToCalibrate}");
                    this.ParentStateMachine.ChangeState(new CalibrateAxisErrorState(this.ParentStateMachine, this.axisToCalibrate, this.calibration, this.InverterStatus, this.Logger));
                }

                speedValue = axisParameters.HomingFastSpeed;
                fastSpeed = this.ParentStateMachine.GetRequiredService<IInvertersProvider>().ConvertMillimetersToPulses(
                    speedValue,
                    Orientation.Vertical);
            }

            if (isCommandToSend)
            {
                var inverterMessage = new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    (short)InverterParameterId.HomingFastSpeed,
                    fastSpeed);

                this.Logger.LogDebug($"Set Homing Fast Speed={speedValue} mm/s [{fastSpeed} impulses], Axis={this.axisToCalibrate}");

                this.ParentStateMachine.EnqueueCommandMessage(inverterMessage);
            }
        }

        #endregion
    }
}
