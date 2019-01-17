using System;
using System.Threading;
using Ferretto.VW.RemoteIODriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    public class CalibrateAxes : ICalibrateAxes
    {
        // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
        public delegate void CalibrateAxesEndEventHandler();

        public delegate void CalibrateAxisEndEventHandler(string calibrationEndMessage);

        public delegate void SwitchVerticalToHorizontalEndEventHandler();

        public delegate void SwitchHorizontalToVerticalEndEventHandler();

        public delegate void StopCalibration(string stopMessage);

        // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
        public delegate void CalibrateAxesErrorEventHandler(string errorDescription);

        #region Fields

        private int m;

        private short ofs;

        private short vFast;

        private short vCreep;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private SwitchMotors switchMotors;

        private CalibrateAxis calibrateAxis;

        private int stepCounter;

        private bool stopPushed;

        // Inverter driver
        private InverterDriver.InverterDriver inverterDriver;

        // RemoteIO
        private IRemoteIO remoteIO;

        #endregion Fields

        #region Events

        // [Ended] event
        public event CalibrateAxesEndEventHandler ThrowEndEvent;

        public event CalibrateAxisEndEventHandler ThrowCalibrationEndEvent;

        public event SwitchVerticalToHorizontalEndEventHandler ThrowSwitchVerticalToHorizontalEndEvent;

        public event SwitchHorizontalToVerticalEndEventHandler ThrowHorizontalToVerticalEndEvent;

        public event StopCalibration ThrowStopEvent;

        // [Error] event
        public event CalibrateAxesErrorEventHandler ThrowErrorEvent;

        #endregion Events

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

        #endregion Properties

        #region Methods

        public void SetAxesOrigin(int m, short ofs, short vFast, short vCreep)
        {
            this.m = m;

            this.ofs = ofs;

            this.vFast = vFast;

            this.vCreep = vCreep;

            this.stepCounter = 0;

            this.stopPushed = false;

            logger.Log(LogLevel.Debug, "Start total routine to calibrate...");

            stepExecution();
        }

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
            this.calibrateAxis.GetAndSetActualcalibrationAxis = "V"; // We assign the Vertical Engine as default

            // Subscribe the event handlers
            this.calibrateAxis.ThrowErrorEvent += this.happenedErrorEvent;
            this.calibrateAxis.ThrowEndEvent += this.nextStep;
            this.switchMotors.ThrowEndEvent += this.nextStep;

            logger.Log(LogLevel.Debug, "Initialize - End");
        }

        private void happenedErrorEvent(CalibrationStatus ErrorDescription)
        {
            // Code to signal an Error
            ThrowErrorEvent?.Invoke(ErrorDescription.ToString());
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
            catch (Exception ex)
            {
                result = false;
            }

            if (!result)
                stopMessage = "An error happened during the STOP";

            ThrowStopEvent?.Invoke(stopMessage);

        }

        private void nextStep()
        {
            logger.Log(LogLevel.Debug, String.Format("Aknowledge of end calibration  stepCounter = {0}", this.stepCounter));

            if (!this.stopPushed)
            {
                stepCounter++;
                this.stepExecution();
            }
        }

        private void stepExecution()
        {
            logger.Log(LogLevel.Debug, String.Format("Execute Step counter = {0}", this.stepCounter));

            switch (stepCounter)
            {

                // Insert here the slow chain motion to find the horizontal cam

                // First Vertical to Horizontal Switch
                case 0:
                    {
                        logger.Log(LogLevel.Debug, "Switch to horizontal motor");
                        // Switch from the Vertical to the Horizontal motor
                        this.switchMotors.callSwitchVertToHoriz();
                        this.switchMotors.SetCurrentMotor = true;

                        break;
                    }
                // First Horizontal Axis Calibration
                case 1:
                    {
                        logger.Log(LogLevel.Debug, "Start calibrate horizontal axis...");
                        ThrowSwitchVerticalToHorizontalEndEvent?.Invoke(); // Throw an event to signal the switch end
                        this.calibrateAxis.Initialize();
                        this.calibrateAxis.GetAndSetActualcalibrationAxis = "H";
                        this.calibrateAxis.SetAxisOrigin(m, ofs, vFast, vCreep);

                        break;
                    }
                // First Horizontal to Vertical Switch
                case 2:
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
                // Vertical Axis Calibration
                case 3:
                    {
                        this.ThrowHorizontalToVerticalEndEvent?.Invoke();
                        logger.Log(LogLevel.Debug, "Start calibrate vertical axis...");
                        this.calibrateAxis.Initialize();
                        this.calibrateAxis.GetAndSetActualcalibrationAxis = "V";
                        // Vertical Homing
                        this.calibrateAxis.SetAxisOrigin(m, ofs, vFast, vCreep);

                        break;
                    }
                // Second Vertical to Horizontal Switch
                case 4:
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
                case 5: 
                    {
                        ThrowSwitchVerticalToHorizontalEndEvent?.Invoke(); // Throw an event to signal the switch end
                        logger.Log(LogLevel.Debug, "Start calibrate horizontal axis (2)...");
                        this.calibrateAxis.Initialize();
                        this.calibrateAxis.GetAndSetActualcalibrationAxis = "H";
                        this.calibrateAxis.SetAxisOrigin(m, ofs, vFast, vCreep);

                        break;
                    }
                // Second Horizontal to Vertical Switch
                case 6:
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

        public void Terminate()
        {
            // Subscribe the event handlers
            this.calibrateAxis.ThrowEndEvent -= this.nextStep;
            this.calibrateAxis.ThrowErrorEvent -= this.happenedErrorEvent;
            this.switchMotors.ThrowEndEvent -= this.nextStep;
        }

        #endregion Methods

    }
}
