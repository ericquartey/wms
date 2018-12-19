namespace Ferretto.VW.ActionBlocks
{
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
}
