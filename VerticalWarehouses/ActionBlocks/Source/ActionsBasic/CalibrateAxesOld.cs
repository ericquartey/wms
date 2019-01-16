using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.RemoteIODriver;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void CalibrateAxesEndedEventHandler(string endMessage);

    // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void CalibrateAxesErrorEventHandler(string errorMessage);

    public class CalibrateAxesOld
    {
        #region Fields

        // Local variable for the properties
        private InverterDriver.InverterDriver inverterDriver; // class instance

        private IRemoteIO remoteIO; // interface

        // Local variable for the objects
        private CalibrateHorizontalAxis calibrateHA;

        private SwitchMotors switchMotors;

        private CalibrateVerticalAxis calibrateVA;

        // Variables for the Axes Calibration, for the future development
        private int m;
        private short ofs;
        private short vFast;
        private short vCreep;

        // Inner variable to loop in the step for the Axes Calibration
        private int calibrationStep;

        #endregion Fields

        #region Events

        // [Ended] event
        public event CalibrateAxesEndedEventHandler ThrowEndEvent;

        // [Error] event
        public event CalibrateAxesErrorEventHandler ThrowErrorEvent;

        #endregion Events

        // Assign the properties before to call the Initialize Method
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }
        public IRemoteIO SetRemoteIOInterface
        {
            set => this.remoteIO = value;
        }

        public void Initialize(int m, short ofs, short vFast, short vCreep)
        {
            // Assign the default parameters to the Calibration methods, for the future development
            this.m = m;
            this.ofs = ofs;
            this.vFast = vFast;
            this.vCreep = vCreep;

            // Calibrate Horizontal Axis set up
            calibrateHA = new CalibrateHorizontalAxis();
            calibrateHA.SetInverterDriverInterface = inverterDriver;
            calibrateHA.Initialize();

            // SwitchMotors set up
            switchMotors = new SwitchMotors();
            switchMotors.SetInverterDriverInterface = inverterDriver;
            switchMotors.SetRemoteIOInterface = remoteIO;
            switchMotors.Initialize(); // Verify this statement because the method is empty

            // Calibrate Vertical Axis set up
            calibrateVA = new CalibrateVerticalAxis();
            calibrateVA.SetInverterDriverInterface = inverterDriver;
            calibrateVA.Initialize();

            // InverterDriver Initialization()
            bool success = this.inverterDriver.Initialize();

            if (!success)
                ThrowErrorEvent?.Invoke("Error during the InverterDriver initialization");

            // Insert here the subscription to the Horizontal and Vertical events
            this.calibrateHA.ThrowEndEvent += this.nextStep;
            this.calibrateVA.ThrowEndEvent += this.nextStep;
            this.switchMotors.ThrowEndEvent += this.nextStep;

            this.calibrateHA.ThrowErrorEvent += this.wrongStep;
            this.calibrateVA.ThrowErrorEvent += this.wrongStep;
            this.switchMotors.ThrowErrorEvent += this.wrongStep;
        }

        public void StartCalibration()
        {
            // calibrationStep = 2; // temporary we start from 2 and no from 0, because the 1st and 2nd steps are not ready
            // stepExecution();

            // this.calibrateHA.SetHAxisOrigin(m, ofs, vFast, vCreep);
            calibrateHA.SetHAxisOrigin(m, ofs, vFast, vCreep);

            ThrowEndEvent?.Invoke("Horizontal calibration");
        }

        private void stepExecution()
        {
            switch(calibrationStep)
            {
                case 0:
                    // Insert here the call to the method to get active engine
                    // and eventually switch to the horizontal engine

                    break;
                case 1:
                    // Insert here the slow chain movement to find the horizontal cam

                    break;
                case 2:
                    // Insert here the call to the Horizontal Homing Calibration
                    calibrateHA.SetHAxisOrigin(m, ofs, vFast, vCreep);

                    break;
                case 3:
                    // Stop the Horizontal Calibration
                    this.HStopButton();

                    // Insert here the switch to the Vertical Engine
                    switchMotors.callSwitchHorizToVert();
                    break;
                case 4:

                    // Insert here the call to the Vertical Homing Calibration
                    calibrateVA.SetVAxisOrigin(m, ofs, vFast, vCreep);

                    break;
                case 5:
                    // Insert here the switch to the Horizontal Engine
                    switchMotors.callSwitchHorizToVert();

                    break;
                case 6:
                    // Insert here the call to the Horizontal Homing Calibration
                    // To do the Axes calibration, we have to calibrate the Horizontal Axis two times
                    calibrateHA.SetHAxisOrigin(m, ofs, vFast, vCreep);

                    break;
                case 7:
                    // Stop the Horizontal Calibration
                    this.HStopButton();

                    // The calibration reaches the end, i throw the End Event
                    ThrowEndEvent?.Invoke("Calibration ended");

                    break;
                default:
                    ThrowErrorEvent?.Invoke("Calibration Error during the step execution");

                    break;
            }
        }

        public void HStopButton()
        {
            bool resturnStop = this.calibrateHA.StopInverter();
            this.calibrateHA.Terminate();

            if (resturnStop)
                ThrowErrorEvent?.Invoke("Horizontal calibration stopped");
            else
                ThrowErrorEvent?.Invoke("Horizontal calibration NOT stopped");
        }

        public void VStopButton()
        {
            this.calibrateVA.StopInverter();
            this.calibrateVA.Terminate();

            ThrowErrorEvent?.Invoke("Vertical calibration stopped");
        }

        public void nextStep()
        {
            //calibrationStep++;
            //stepExecution();

            ThrowEndEvent?.Invoke("Horizontal calibration ended");
        }

        public void wrongStep(CalibrationStatus errorDescription)
        {
            ThrowErrorEvent?.Invoke("Calibration Error during the step execution");
        }
        public void wrongStep()
        {
            ThrowErrorEvent?.Invoke("Error during the Vertical/Horizontal calibration");
        }

        public void Terminate()
        {
            calibrateHA.Terminate();
            switchMotors.Terminate(); // Verify this statement because the method is empty
            calibrateVA.Terminate();

            // Insert here the unsubscription to the End events
            this.calibrateHA.ThrowEndEvent -= this.nextStep;
            this.calibrateVA.ThrowEndEvent -= this.nextStep;
            this.switchMotors.ThrowEndEvent -= this.nextStep;

            // Insert here the subscription to the Error events
            this.calibrateHA.ThrowErrorEvent -= this.wrongStep;
            this.calibrateVA.ThrowErrorEvent -= this.wrongStep;
            this.switchMotors.ThrowErrorEvent -= this.wrongStep;
        }
    }
}
