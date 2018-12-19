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
    /// Interface for Drawer weight detection.
    /// </summary>
    public interface IDrawerWeightDetection
    {
        #region Properties

        /// <summary>
        /// Set positioning interface.
        /// </summary>
        PositioningDrawer SetPositioningDrawerInterface { set; }

        /// <summary>
        /// Get/Set the weight of drawer.
        /// </summary>
        float Weight { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the weight detection routine.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Run the routine to detect the weight.
        /// </summary>
        /// <param name="d">Target position (relative)</param>
        /// <param name="v">Speed</param>
        /// <param name="acc">Acceleration</param>
        /// <param name="dec">Deceleration</param>
        void Run(decimal d, float v, float acc, float dec);

        /// <summary>
        /// Terminate the weight detection routine.
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
        /// Get current position.
        /// </summary>
        int CurrentPosition { get; }

        /// <summary>
        /// Enable read maximum analog Ic (absorption current).
        /// </summary>
        bool EnableReadMaxAnalogIc { set; }

        /// <summary>
        /// Enable the retrivial position of drawer during movement.
        /// </summary>
        bool EnableRetrivialCurrentPositionMode { set; }

        /// <summary>
        /// Get the maximum Analog Ic (absorption current).
        /// </summary>
        short MaxAnalogIc { get; }

        /// <summary>
        /// Set Inverter driver.
        /// </summary>
        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        #endregion Properties

        #region Methods

        void Initialize();

        /// <summary>
        /// Move along vertical axis.
        /// </summary>
        /// <param name="x">Target position</param>
        /// <param name="vMax">Speed.</param>
        /// <param name="acc">Acceleration</param>
        /// <param name="dec">Deceleration</param>
        /// <param name="w">Weight</param>
        /// <param name="offset">Offset (distance)</param>
        void MoveAlongVerticalAxisToPoint(decimal x, float vMax, float acc, float dec, float w, short offset);

        void Stop();

        void Terminate();

        #endregion Methods
    }
}
