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

            this.driver.Initialize();
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.driver.Terminate();
        }

        public void ExecuteHorizontalHoming()
        {
            return;
        }

        public void ExecuteHorizontalPosition()
        {
            return;
        }

        public void ExecuteVerticalHoming()
        {
            var calibration = new CalibrateAxis();
            calibration.SetInverterDriverInterface = this.driver;
            calibration.Initialize();

            calibration.ActualCalibrationAxis = CalibrationType.VERTICAL_CALIBRATION;
            calibration.SetAxisOrigin();
        }

        public void ExecuteVerticalPosition(int target, float weight)
        {
            return;
        }

        public float GetDrawerWeight()
        {
            return 0.0f;
        }

        public bool[] GetSensorsStates()
        {
            return null;
        }

        #endregion Methods
    }
}
