namespace Ferretto.VW.ActionBlocks
{
    public interface ICalibrateAxis
    {
        #region Properties

        /// <summary>
        /// Set inverter driver.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        CalibrationType ActualCalibrationAxis { set; get;  }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        void Initialize();

        void SetUpVerticalHomingParameters(int acc, int vFast, int vCreep);

        /// <summary>
        /// Start Calibrate Vertical Axis routine.
        /// </summary>
        void SetAxisOrigin();

        /// <summary>
        /// Stop the routine.
        /// </summary>
        bool StopInverter();

        /// <summary>
        /// Terminate the Calibrate Vertical Axis routine.
        /// </summary>
        void Terminate();

        #endregion Methods
    }
}
