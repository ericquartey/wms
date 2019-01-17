namespace Ferretto.VW.ActionBlocks
{
    public interface ICalibrateAxis
    {
        #region Properties

        /// <summary>
        /// Set inverter driver.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        string GetAndSetActualcalibrationAxis { set; get;  }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start Calibrate Vertical Axis routine.
        /// </summary>
        void SetAxisOrigin(int m, short ofs, short vFast, short vCreep);

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
