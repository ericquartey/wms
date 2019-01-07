
namespace Ferretto.VW.ActionBlocks
{
    /// <summary>
    /// Interface for Calibrate Horizontal Axis routine.
    /// </summary>
    public interface ICalibrateHorizontalAxis
    {
        #region Properties

        /// <summary>
        /// Set inverter driver.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Horizontal Axis routine.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start Calibrate Horizontal Axis routine.
        /// </summary>
        void SetHAxisOrigin(int m, short ofs, short vFast, short vCreep);

        /// <summary>
        /// Stop the routine.
        /// </summary>
        bool StopInverter();

        /// <summary>
        /// Terminate the Calibrate Horizontal Axis routine.
        /// </summary>
        void Terminate();

        #endregion Methods
    }
}
