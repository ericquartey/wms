using System;
using Ferretto.VW.RemoteIODriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    public class CalibrateAxes : ICalibrateAxes
    {
        #region Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private int acc;

        private CalibrateAxis calibrateAxis;

        // Inverter driver
        private InverterDriver.InverterDriver inverterDriver;

        // RemoteIO
        private IRemoteIO remoteIO;

        private int stepCounter;

        private bool stopPushed;

        private SwitchMotors switchMotors;

        private int vCreep;

        private int vFast;

        #endregion

        #region Delegates

        // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
        public delegate void CalibrateAxesEndEventHandler();

        // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
        public delegate void CalibrateAxesErrorEventHandler(string errorDescription);

        public delegate void CalibrateAxisEndEventHandler(string calibrationEndMessage);

        public delegate void SetUpVerticalHomingEventHandler();

        public delegate void StopCalibrationEventHandler(string stopMessage);

        public delegate void SwitchHorizontalToVerticalEndEventHandler();

        public delegate void SwitchVerticalToHorizontalEndEventHandler();

        #endregion

        #region Events

        public event CalibrateAxisEndEventHandler ThrowCalibrationEndEvent;

        // [Ended] event
        public event CalibrateAxesEndEventHandler ThrowEndEvent;

        // [Error] event
        public event CalibrateAxesErrorEventHandler ThrowErrorEvent;

        public event SwitchHorizontalToVerticalEndEventHandler ThrowHorizontalToVerticalEndEvent;

        public event SetUpVerticalHomingEventHandler ThrowSetUpVerticalHomingEndEvent;

        public event StopCalibrationEventHandler ThrowStopEvent;

        public event SwitchVerticalToHorizontalEndEventHandler ThrowSwitchVerticalToHorizontalEndEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Set Inverter driver.
        /// </summary>
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        public IRemoteIO SetRemoteIOInterface
        {
            set => this.remoteIO = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        public void Initialize()
        {
            logger.Log(LogLevel.Debug, "Initialize - Begin");

            // Insert here the SwitchMotors class
            this.switchMotors = new SwitchMotors();
            this.switchMotors.SetInverterDriverInterface = this.inverterDriver;
            this.switchMotors.SetRemoteIOInterface = this.remoteIO;
            this.switchMotors.SetCurrentMotor = false;

            // Calibrate Axis class
            this.calibrateAxis = new CalibrateAxis();
            this.calibrateAxis.SetInverterDriverInterface = this.inverterDriver;
            this.calibrateAxis.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION; // We assign the Vertical Engine as default

            // Subscribe the event handlers
            this.calibrateAxis.ThrowErrorEvent += this.happenedErrorEvent;
            this.calibrateAxis.ThrowEndEvent += this.nextStep;
            this.switchMotors.ThrowEndEvent += this.nextStep;
            this.calibrateAxis.ThrowSetUpEnd += this.nextStep;

            logger.Log(LogLevel.Debug, "Initialize - End");
        }

        public void SetAxesOrigin(int acc, int vFast, int vCreep)
        {
            this.acc = acc;

            this.vFast = vFast;

            this.vCreep = vCreep;

            this.stepCounter = 0;

            this.stopPushed = false;

            logger.Log(LogLevel.Debug, "Start total routine to calibrate...");

            stepExecution();
        }

        public void StopInverter()
        {
            bool result;
            string stopMessage = "Calibration Stopped";

            try
            {
                result = this.calibrateAxis.StopInverter();
                this.stopPushed = true;
            }
            catch (Exception)
            {
                result = false;
            }

            if (!result)
                stopMessage = "An error happened during the STOP";

            ThrowStopEvent?.Invoke(stopMessage);
        }

        public void Terminate()
        {
            // Unsubscribe the event handlers
            this.calibrateAxis.ThrowEndEvent -= this.nextStep;
            this.calibrateAxis.ThrowErrorEvent -= this.happenedErrorEvent;
            this.switchMotors.ThrowEndEvent -= this.nextStep;
            this.calibrateAxis.ThrowSetUpEnd -= this.nextStep;
        }

        private void happenedErrorEvent(CalibrationStatus ErrorDescription)
        {
            // Code to signal an Error
            ThrowErrorEvent?.Invoke(ErrorDescription.ToString());
        }

        private void nextStep()
        {
            logger.Log(LogLevel.Debug, string.Format("Aknowledge of end calibration  stepCounter = {0}", this.stepCounter));

            if (!this.stopPushed)
            {
                stepCounter++;
                this.stepExecution();
            }
        }

        private void stepExecution()
        {
            logger.Log(LogLevel.Debug, string.Format("Execute Step counter = {0}", this.stepCounter));

            switch (stepCounter)
            {
                // Insert here the slow chain motion to find the horizontal cam

                // SetUp Vertical Axis Calibration Parameters
                case 0:
                    {
                        logger.Log(LogLevel.Debug, "SetUp vertical calibration parameters");
                        this.calibrateAxis.Initialize();
                        this.calibrateAxis.SetUpVerticalHomingParameters(acc, vFast, vCreep);

                        break;
                    }
                // Switch from the Vertical to the Horizontal motor
                case 1:
                    {
                        // Insert here an event for the end of Set Vertical Homing Parameters
                        ThrowSetUpVerticalHomingEndEvent?.Invoke();
                        this.calibrateAxis.Terminate();
                        logger.Log(LogLevel.Debug, "Switch to horizontal motor");
                        this.switchMotors.callSwitchVertToHoriz();
                        this.switchMotors.SetCurrentMotor = true;

                        break;
                    }
                // First Horizontal Axis Calibration
                case 2:
                    {
                        logger.Log(LogLevel.Debug, "Start calibrate horizontal axis...");
                        ThrowSwitchVerticalToHorizontalEndEvent?.Invoke(); // Throw an event to signal the switch end
                        this.calibrateAxis.Initialize();
                        this.calibrateAxis.ActualCalibrationAxis = CalibrationType.HORIZONTAL_CALIBRATION;
                        this.calibrateAxis.SetAxisOrigin();

                        break;
                    }
                // First Horizontal to Vertical Switch
                case 3:
                    {
                        logger.Log(LogLevel.Debug, "Terminate horizontal axis calibration.");
                        this.calibrateAxis.Terminate();
                        // Notify horizontal calibration end (first)
                        this.ThrowCalibrationEndEvent?.Invoke("Fist Horizontal Calibration");
                        // Switch from the Horizontal to the Vertical motor
                        logger.Log(LogLevel.Debug, "Switch to vertical motor");
                        this.switchMotors.callSwitchHorizToVert();
                        this.switchMotors.SetCurrentMotor = false;

                        break;
                    }
                // Start Vertical Axis Calibration
                case 4:
                    {
                        this.ThrowHorizontalToVerticalEndEvent?.Invoke();
                        logger.Log(LogLevel.Debug, "Start calibrate vertical axis...");
                        this.calibrateAxis.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION;
                        this.calibrateAxis.Initialize();
                        // Vertical Homing
                        this.calibrateAxis.SetAxisOrigin();

                        break;
                    }
                // Second Vertical to Horizontal Switch
                case 5:
                    {
                        logger.Log(LogLevel.Debug, "Terminate vertical axis calibration.");
                        this.calibrateAxis.Terminate();
                        // Notify vertical calibration end
                        this.ThrowCalibrationEndEvent?.Invoke("Vertical Calibration");
                        // Switch from the Vertical to the Horizontal motor
                        logger.Log(LogLevel.Debug, "Switch to horizontal motor");
                        this.switchMotors.callSwitchVertToHoriz();
                        this.switchMotors.SetCurrentMotor = true;

                        break;
                    }
                // Second Horizontal Axis Calibration
                case 6:
                    {
                        ThrowSwitchVerticalToHorizontalEndEvent?.Invoke(); // Throw an event to signal the switch end
                        logger.Log(LogLevel.Debug, "Start calibrate horizontal axis (2)...");
                        this.calibrateAxis.Initialize();
                        this.calibrateAxis.ActualCalibrationAxis = CalibrationType.HORIZONTAL_CALIBRATION;
                        this.calibrateAxis.SetAxisOrigin();

                        break;
                    }
                // Second Horizontal to Vertical Switch
                case 7:
                    {
                        logger.Log(LogLevel.Debug, "Terminate horizontal axis calibration (2).");
                        this.calibrateAxis.Terminate();
                        // Notify horizontal calibration end (last)
                        this.ThrowCalibrationEndEvent?.Invoke("Second Horizontal Calibration");
                        // Switch from the Horizontal to the Vertical motor
                        logger.Log(LogLevel.Debug, "Switch to vertical motor");
                        this.switchMotors.callSwitchHorizToVert();
                        this.switchMotors.SetCurrentMotor = false;

                        break;
                    }
                default:
                    {
                        this.ThrowHorizontalToVerticalEndEvent?.Invoke();
                        // Code to signal the End Event
                        ThrowEndEvent?.Invoke();

                        break;
                    }
            }
        }

        #endregion
    }
}
