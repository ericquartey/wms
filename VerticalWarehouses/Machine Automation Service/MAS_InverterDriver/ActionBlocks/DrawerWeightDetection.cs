using Ferretto.VW.MAS_InverterDriver.Interface;

namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class DrawerWeightDetection : IInverterActions
    {
        #region Fields

        private float acc;
        private float dec;
        private PositioningDrawer drawerPositionController;
        private bool executeWeighting;
        private float speed;
        private int target;

        #endregion Fields

        #region Constructors

        public DrawerWeightDetection()
        {
            this.target = 0;
            this.speed = 0.0f;
            this.acc = 0.0f;
            this.dec = 0.0f;
            this.executeWeighting = false;
        }

        #endregion Constructors

        #region Events

        public event EndEventHandler EndEvent;
        public event ErrorEventHandler ErrorEvent;

        #endregion Events

        #region Properties

        public PositioningDrawer SetPositioningDrawerInterface
        {
            set => this.drawerPositionController = value;
        }

        public float Weight { get; set; }

        public Ferretto.VW.InverterDriver.IInverterDriver SetInverterDriverInterface
        {
            set => this.drawerPositionController.SetInverterDriverInterface = value;
        }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            this.Weight = -1.0f;

            if (this.drawerPositionController != null)
            {
                this.drawerPositionController.EndEvent += this.DrawerPositioningEndEvent;
                this.drawerPositionController.ErrorEvent += this.DrawerPositioningErrorEvent;
            }
        }

        public void RestorePosition()
        {
            this.drawerPositionController.EnableReadMaxAnalogIc = false;  

            this.executeWeighting = false;

            this.drawerPositionController?.MoveAlongVerticalAxisToPoint(-this.target, this.speed, this.acc, this.dec, -1, 0);
        }

        public void Run(int targetPosition, float v, float acc, float dec)
        {
            this.drawerPositionController.AbsoluteMovement = false;    
            this.drawerPositionController.EnableReadMaxAnalogIc = true;   

            this.target = targetPosition;
            this.speed = v;
            this.acc = acc;
            this.dec = dec;

            this.executeWeighting = true;
            this.drawerPositionController?.MoveAlongVerticalAxisToPoint(targetPosition, v, acc, dec, -1, 0);
        }

        public void Stop()
        {
            this.drawerPositionController?.Stop();
        }

        public void Terminate()
        {
            this.drawerPositionController?.Stop();

            if (this.drawerPositionController != null)
            {
                this.drawerPositionController.EndEvent -= this.DrawerPositioningEndEvent;
                this.drawerPositionController.ErrorEvent -= this.DrawerPositioningErrorEvent;
            }
        }

        private float ConvertToWeight(long value)
        {
            return value;
        }

        private void DrawerPositioningEndEvent()
        {
            if (this.executeWeighting)
            {
                this.Weight = this.ConvertToWeight(this.drawerPositionController.MaxAnalogIc);
            }

            this.EndEvent?.Invoke();
        }

        private void DrawerPositioningErrorEvent()
        {
            this.ErrorEvent?.Invoke();
        }

        #endregion Methods
    }
}
