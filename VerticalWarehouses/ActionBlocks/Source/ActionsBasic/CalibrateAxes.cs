using System;
using System.Threading;
using Ferretto.VW.RemoteIODriver;
using NLog;

namespace Ferretto.VW.ActionBlocks
{
    public class CalibrateAxes
    {
        // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
        public delegate void CalibrateAxesEndEventHandler();

        public delegate void CalibrateHAxisEndEventHandler(int stepCounter);

        public delegate void CalibrateVAxisEndEventHandler();

        public delegate void SwitchVerticalToHorizontalEndEventHandler();

        public delegate void SwitchHorizontalToVerticalEndEventHandler();

        // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
        public delegate void CalibrateAxesErrorEventHandler(string errorDescription);

        #region Fields

        private int m;

        private short ofs;

        private short vFast;

        private short vCreep;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CalibrateHorizontalAxis calibrateHorizontalAxis;

        private SwitchMotors switchMotors;

        private CalibrateVerticalAxis calibrateVerticalAxis;

        private int stepCounter;

        // Inverter driver
        private InverterDriver.InverterDriver inverterDriver;

        // RemoteIO
        private IRemoteIO remoteIO;

        #endregion Fields

        #region Events

        // [Ended] event
        public event CalibrateAxesEndEventHandler ThrowEndEvent;

        public event CalibrateHAxisEndEventHandler ThrowHorizontalCalibrationEndEvent;

        public event CalibrateVAxisEndEventHandler ThrowVerticalCalibrationEndEvent;

        public event SwitchVerticalToHorizontalEndEventHandler ThrowSwitchVerticalToHorizontalEndEvent;

        public event SwitchHorizontalToVerticalEndEventHandler ThrowHorizontalToVerticalEndEvent;

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

        public void SetAxesOrigin(int m, short ofs, short vFast, short vCreep)
        {
            this.m = m;

            this.ofs = ofs;

            this.vFast = vFast;

            this.vCreep = vCreep;

            this.stepCounter = 0;

            logger.Log(LogLevel.Debug, "Start total routine to calibrate...");

            stepExecution();
        }

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        public void Initialize()
        {
            // Insert here the SwitchMotors class
            this.switchMotors = new SwitchMotors();
            this.switchMotors.SetInverterDriverInterface = this.inverterDriver;
            this.switchMotors.SetRemoteIOInterface = this.remoteIO;
            this.switchMotors.SetCurrentMotor = false;

            // Calibration Horizontal Axis class
            this.calibrateHorizontalAxis = new CalibrateHorizontalAxis();
            this.calibrateHorizontalAxis.SetInverterDriverInterface = this.inverterDriver;

            // Calibration Vertical Axis class
            this.calibrateVerticalAxis = new CalibrateVerticalAxis();
            this.calibrateVerticalAxis.SetInverterDriverInterface = this.inverterDriver;

            // Subscribe the event handlers
            this.calibrateHorizontalAxis.ThrowEndEvent += this.nextStep;
            this.calibrateHorizontalAxis.ThrowErrorEvent += this.happenedErrorEvent;
            this.switchMotors.ThrowEndEvent += this.nextStep;
            this.calibrateVerticalAxis.ThrowEndEvent += this.nextStep;
        }

        private void happenedErrorEvent(CalibrationStatus ErrorDescription)
        {
            // Code to signal an Error
            ThrowErrorEvent?.Invoke(ErrorDescription.ToString());
        }

        public bool StopInverter()
        {
            bool result;

            try
            {
                if (this.switchMotors.SetCurrentMotor)
                {
                    result = this.calibrateHorizontalAxis.StopInverter();
                }
                else
                {
                    result = this.calibrateVerticalAxis.StopInverter();
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private void nextStep()
        {
            logger.Log(LogLevel.Debug, String.Format("Aknowledge of end calibration  stepCounter={0}", this.stepCounter));

            stepCounter++;
            this.stepExecution();
        }

        private void stepExecution()
        {
            logger.Log(LogLevel.Debug, String.Format("Execute Step counter = {0}", this.stepCounter));

            switch (stepCounter)
            {

                // Insert here the slow chain motion to find the horizontal cam
                case 0:
                    {
                        logger.Log(LogLevel.Debug, "Switch to horizontal motor");
                        // Switch from the Vertical to the Horizontal motor
                        this.switchMotors.callSwitchVertToHoriz();
                        this.switchMotors.SetCurrentMotor = true;

                        break;
                    }
                case 1: // First Horizontal Axis Calibration
                    {
                        logger.Log(LogLevel.Debug, "Calibrate horizontal axis...");
                        ThrowSwitchVerticalToHorizontalEndEvent?.Invoke(); // Throw an event to signal the switch end
                        this.calibrateHorizontalAxis.Initialize();
                        this.calibrateHorizontalAxis.SetHAxisOrigin(m, ofs, vFast, vCreep);

                        break;
                    }
                case 2: // First Horizontal Axis Calibration
                    {
                        logger.Log(LogLevel.Debug, "Terminate horizontal axis calibration.");
                        this.calibrateHorizontalAxis.Terminate();
                        // Notify horizontal calibration end (first)
                        this.ThrowHorizontalCalibrationEndEvent?.Invoke(stepCounter);
                        // Switch from the Horizontal to the Vertical motor
                        logger.Log(LogLevel.Debug, "Switch to vertical motor");
                        this.switchMotors.callSwitchHorizToVert();
                        this.switchMotors.SetCurrentMotor = false;

                        break;
                    }
                case 3:
                    {
                        this.ThrowHorizontalToVerticalEndEvent?.Invoke();
                        logger.Log(LogLevel.Debug, "Calibrate vertical axis...");
                        this.calibrateVerticalAxis.Initialize();
                        // Vertical Homing
                        this.calibrateVerticalAxis.SetVAxisOrigin(m, ofs, vFast, vCreep);

                        break;
                    }
                case 4:
                    {
                        logger.Log(LogLevel.Debug, "Terminate vertical axis calibration.");
                        this.calibrateVerticalAxis.Terminate();
                        // Notify vertical calibration end
                        this.ThrowVerticalCalibrationEndEvent?.Invoke();
                        // Switch from the Vertical to the Horizontal motor
                        logger.Log(LogLevel.Debug, "Switch to horizontal motor");
                        this.switchMotors.callSwitchVertToHoriz();
                        this.switchMotors.SetCurrentMotor = true;

                        break;
                    }
                case 5: // Second Horizontal Axis Calibration
                    {
                        ThrowSwitchVerticalToHorizontalEndEvent?.Invoke(); // Throw an event to signal the switch end
                        logger.Log(LogLevel.Debug, "Calibrate horizontal axis (2)...");
                        this.calibrateHorizontalAxis.Initialize();
                        this.calibrateHorizontalAxis.SetHAxisOrigin(m, ofs, vFast, vCreep);

                        break;
                    }
                case 6:
                    {
                        logger.Log(LogLevel.Debug, "Terminate horizontal axis calibration (2).");
                        this.calibrateHorizontalAxis.Terminate();
                        // Notify horizontal calibration end (last)
                        this.ThrowHorizontalCalibrationEndEvent?.Invoke(stepCounter); 
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
            this.calibrateHorizontalAxis.ThrowEndEvent -= this.nextStep;
            this.calibrateHorizontalAxis.ThrowErrorEvent -= this.happenedErrorEvent;
            this.switchMotors.ThrowEndEvent -= this.nextStep;
            this.calibrateVerticalAxis.ThrowEndEvent -= this.nextStep;
        }

        #endregion Methods

    }
}
