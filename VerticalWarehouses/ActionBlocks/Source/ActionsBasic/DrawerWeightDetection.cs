namespace Ferretto.VW.ActionBlocks
{
    // On [EndEventHandler] delegate for Drawer weight detection routine
    public delegate void DrawerWeightDetectionEndEventHandler(bool result);

    // On [ErrorEventHandler] delegate for Drawer weight detection routine
    public delegate void DrawerWeightDetectionErrorEventHandler(string error_Message);

    /// <summary>
    /// Drawer weight detection class.
    /// This class handles the weight detection routine. It uses the PositioningDrawer class.
    /// </summary>
    public class DrawerWeightDetection : IDrawerWeightDetection
    {
        // Controller for the drawer positioning movement
        // Set a movement relative - target position: 10 cm - enable the analog signal sampling (current Ic) -
        // store the Ic value in a internal member of class - get the value via class Property

        #region Fields

        private PositioningDrawer drawerPositionController;
        private float weight;

        #endregion Fields

        #region Events

        // [Ended] event
        public event DrawerWeightDetectionEndEventHandler EndEvent;

        // [Error] event
        public event DrawerWeightDetectionErrorEventHandler ErrorEvent;

        #endregion Events

        #region Properties

        /// <summary>
        /// Set the inverter driver interface.
        /// </summary>
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.drawerPositionController.SetInverterDriverInterface = value;
        }

        /// <summary>
        /// Get the weight of drawer.
        /// </summary>
        public float Weight => this.weight;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize the weight detection routine.
        /// </summary>
        public void Initialize()
        {
            this.weight = -1.0f;

            // Instantiate the drawer positioning
            this.drawerPositionController = new PositioningDrawer();
            // Subscribes the event handlers
            this.drawerPositionController.ThrowEndEvent += this.DrawerPositioningEndEvent;
            this.drawerPositionController.ThrowErrorEvent += this.DrawerPositioningErrorEvent;
        }

        /// <summary>
        /// Run the routine to detect the weight.
        /// </summary>
        public void Run(long d, float v, float acc, float dec)
        {
            // Set properties of movement
            // this.drawerPositionController.AbsoluteMovement = false;    // set relative mode positioning
            this.drawerPositionController.EnableReadMaxAnalogIc = true;   // enable the countinuous sampling for current Ic

            // Start the movement
            this.drawerPositionController.MoveAlongVerticalAxisToPoint((short)d, v, acc, dec, -1, 0);
        }

        /// <summary>
        /// Terminate the weight detection routine.
        /// </summary>
        public void Terminate()
        {
            this.drawerPositionController.Stop();
            this.drawerPositionController.ThrowEndEvent -= this.DrawerPositioningEndEvent;
            this.drawerPositionController.ThrowErrorEvent -= this.DrawerPositioningErrorEvent;
        }

        /// <summary>
        /// ...
        /// </summary>
        private float ConvertToWeight(long value)
        {
            return value;
        }

        /// <summary>
        /// Occurs when routine for drawer positioning movement is ended.
        /// </summary>
        private void DrawerPositioningEndEvent(bool result)
        {
            // request the weight to the inverter
            //var exitStatus = this.driver?.SendRequest(InverterDriver.ParameterID.ANALOG_IC_PARAM, 0, 0x04);   // ? Maybe it can be deleted
            //if (exitStatus == InverterDriver.InverterDriverExitStatus.Success)
            //{
            //    this.routineDone = true;
            //}

            if (result)
            {
                this.weight = this.ConvertToWeight(this.drawerPositionController.MaxAnalogIc);        // Specify a property for the ANALOG_IC_PARAM value
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

        /**/

        /// <summary>
        /// Occurs when inverter driver receives a response by the inverter for a submitted SendRequest operation.  // It can be deleted
        /// </summary>
        //private void EnquiryTelegram(System.Object sender, InverterDriver.EnquiryTelegramDoneEventArgs eventArgs)
        //{
        //    if (eventArgs.ParamID == InverterDriver.ParameterID.ANALOG_IC_PARAM)
        //    {
        //        // cache value of weight
        //        var type = eventArgs.Type;
        //        var value = eventArgs.Value;

        //        this.weight = Convert.ToInt16(value);
        //    }

        //    // notify with success only if routine has been done and weight has been cached
        //    this.EndEvent?.Invoke((this.weight != -1.0) && this.routineDone);
        //}

        /**/
    }
}
