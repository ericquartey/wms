namespace Ferretto.VW.ActionBlocks
{
    /// <summary>
    /// Interface for Calibrate Vertical Axis routine.
    /// </summary>
    public interface ICalibrateVerticalAxis
    {
        #region Properties

        /// <summary>
        /// Set inverter driver.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the Calibrate Vertical Axis routine.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Start Calibrate Vertical Axis routine.
        /// </summary>
        void SetVAxisOrigin(int m, short ofs, short vFast, short vCreep);

        /// <summary>
        /// Stop the routine.
        /// </summary>
        void StopInverter();

        /// <summary>
        /// Terminate the Calibrate Vertical Axis routine.
        /// </summary>
        void Terminate();

        #endregion Methods
    }

    /// <summary>
    /// Interface for Positioning (vertical) drawer routine.
    /// </summary>
    public interface IPositioningDrawer
    {
        #region Properties

        /// <summary>
        /// Set Inverter driver.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        #endregion Properties

        #region Methods
        void Initialize();

        void MoveAlongVerticalAxisToPoint(short x, float vMax, float acc, float dec, float w, short offset);

        void Halt();

        void StopInverter();

        void Terminate();

        #endregion Methods
    }
}
