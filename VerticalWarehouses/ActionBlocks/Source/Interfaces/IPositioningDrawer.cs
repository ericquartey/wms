namespace Ferretto.VW.ActionBlocks
{
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
