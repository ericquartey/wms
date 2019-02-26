using System;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface;

namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class DrawerWeightDetection : IInverterActions
    {
        #region Fields

        private Single acc;
        private Single dec;
        private PositioningDrawer drawerPositionController;
        private Boolean executeWeighting;
        private Single speed;
        private Int32 target;

        #endregion

        #region Constructors

        public DrawerWeightDetection()
        {
            this.target = 0;
            this.speed = 0.0f;
            this.acc = 0.0f;
            this.dec = 0.0f;
            this.executeWeighting = false;
        }

        #endregion

        #region Events

        public event EndEventHandler EndEvent;

        public event ErrorEventHandler ErrorEvent;

        #endregion

        #region Properties

        public IInverterDriver SetInverterDriverInterface
        {
            set => this.drawerPositionController.SetInverterDriverInterface = value;
        }

        public PositioningDrawer SetPositioningDrawerInterface
        {
            set => this.drawerPositionController = value;
        }

        public Single Weight { get; set; }

        #endregion

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

            this.drawerPositionController?.MoveAlongVerticalAxisToPoint(-this.target, this.speed, this.acc, this.dec,
                -1, 0);
        }

        public void Run(Int32 targetPosition, Single v, Single acc, Single dec)
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

        private Single ConvertToWeight(Int64 value)
        {
            return value;
        }

        private void DrawerPositioningEndEvent()
        {
            if (this.executeWeighting) this.Weight = this.ConvertToWeight(this.drawerPositionController.MaxAnalogIc);

            this.EndEvent?.Invoke();
        }

        private void DrawerPositioningErrorEvent()
        {
            this.ErrorEvent?.Invoke();
        }

        #endregion
    }
}
