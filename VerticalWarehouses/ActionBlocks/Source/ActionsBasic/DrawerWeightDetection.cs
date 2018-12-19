namespace Ferretto.VW.ActionBlocks
{
    // On [EndEventHandler] delegate for Drawer weight detection routine
    public delegate void DrawerWeightDetectionEndEventHandler(bool result);

    // On [ErrorEventHandler] delegate for Drawer weight detection routine
    public delegate void DrawerWeightDetectionErrorEventHandler(string error_Message);

    /// <summary>
    /// Drawer weight detection class.
    /// This class handles the weight detection routine. It uses the PositioningDrawer class.
    /// The weight is related to the absorption current Ic during the movement
    /// </summary>
    public class DrawerWeightDetection : IDrawerWeightDetection
    {
        // Routine for detect the weight of drawer:
        // 1. Set a movement relative - target position - enable the analog signal sampling (current Ic)
        // 2. Make the movement along vertical axis
        // 3. Store the Ic value in a internal member of class - get the value via class Property

        #region Fields

        private PositioningDrawer drawerPositionController;
        private float weight;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default c-tor.
        /// </summary>
        public DrawerWeightDetection()
        {
        }

        #endregion Constructors

        #region Events

        // [Ended] event
        public event DrawerWeightDetectionEndEventHandler EndEvent;

        // [Error] event
        public event DrawerWeightDetectionErrorEventHandler ErrorEvent;

        #endregion Events

        #region Properties

        /// <summary>
        /// Set the positioning interface.
        /// </summary>
        public PositioningDrawer SetPositioningInterface
        {
            set => this.drawerPositionController = value;
        }

        /// <summary>
        /// Get the weight of drawer.
        /// </summary>
        public float Weight => this.weight;

        /// <summary>
        /// Set the inverter driver interface.
        /// </summary>
        private InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.drawerPositionController.SetInverterDriverInterface = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the weight detection routine.
        /// </summary>
        public void Initialize()
        {
            this.weight = -1.0f;

            // Subscribes the event handlers
            if (this.drawerPositionController != null)
            {
                this.drawerPositionController.ThrowEndEvent += this.DrawerPositioningEndEvent;
                this.drawerPositionController.ThrowErrorEvent += this.DrawerPositioningErrorEvent;
            }
        }

        /// <summary>
        /// Run the routine to detect the weight.
        /// </summary>
        public void Run(decimal d, float v, float acc, float dec)
        {
            // Set properties of movement
            this.drawerPositionController.AbsoluteMovement = false;    // set relative mode positioning
            this.drawerPositionController.EnableReadMaxAnalogIc = true;   // enable the countinuous sampling for current Ic

            // Start the movement
            this.drawerPositionController?.MoveAlongVerticalAxisToPoint(d, v, acc, dec, -1, 0);
        }

        /// <summary>
        /// Terminate the weight detection routine.
        /// </summary>
        public void Terminate()
        {
            // Invoke a stop to positioning object
            this.drawerPositionController?.Stop();

            // Unsubscribes the event handlers
            if (this.drawerPositionController != null)
            {
                // Unsubscribes the event handlers
                this.drawerPositionController.ThrowEndEvent -= this.DrawerPositioningEndEvent;
                this.drawerPositionController.ThrowErrorEvent -= this.DrawerPositioningErrorEvent;
            }
        }

        /// <summary>
        /// ...
        /// </summary>
        private float ConvertToWeight(long value)
        {
            // TODO: Use a mathematical formula to get the weight from the Ic current
            // Actually this formula is not provided
            return value;
        }

        /// <summary>
        /// Occurs when routine for drawer positioning movement is ended.
        /// </summary>
        private void DrawerPositioningEndEvent(bool result)
        {
            if (result)
            {
                this.weight = this.ConvertToWeight(this.drawerPositionController.MaxAnalogIc);
            }

            // notify with success only if routine has been done and weight has been cached
            this.EndEvent?.Invoke(result);
        }

        /// <summary>
        /// Occurs when an error happens (related to routine for drawer positioning movement).
        /// </summary>
        private void DrawerPositioningErrorEvent(string error_Message)
        {
            // pass-through
            this.ErrorEvent?.Invoke(error_Message);
        }

        #endregion Methods
    }
}
