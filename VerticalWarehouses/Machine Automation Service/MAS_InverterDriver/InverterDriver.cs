using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.MAS_InverterDriver
{
    public class InverterDriver : IInverterDriver
    {
        #region Fields

        private Ferretto.VW.InverterDriver.InverterDriver driver;

        #endregion Fields

        #region Constructors

        public InverterDriver()
        {
            this.driver = new VW.InverterDriver.InverterDriver();

            // connect to inverter (device)
            this.driver.Initialize();
        }

        #endregion Constructors

        #region Methods
        public void ExecuteVerticalHoming()
        {
            Ferretto.VW.InverterDriver.CalibrateAxis calibration = new CalibrateAxis();
            calibration.SetInverterDriverInterface = this.driver;
            calibration.Initialize();

            calibration.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION;
            // Do the calibration
            calibration.SetAxisOrigin();

        }

        public void ExecuteHorizontalHoming()
        {
            return;
        }

        public void ExecuteVerticalPosition(int target, float weight)
        {
            return;
        }

        public void ExecuteHorizontalPosition()
        {
            return;
        }

        public bool[] GetSensorsStates()
        {
            return null;
        }

        public float GetDrawerWeight()
        {
            return 0.0f;
        }

        #endregion Methods
    }
}
