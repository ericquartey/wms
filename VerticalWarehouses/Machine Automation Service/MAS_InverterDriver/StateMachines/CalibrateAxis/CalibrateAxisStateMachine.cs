using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class CalibrateAxisStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private Axis currentAxis;

        private bool disposed;

        #endregion

        #region Constructors

        public CalibrateAxisStateMachine(Axis axisToCalibrate, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.inverterCommandQueue = inverterCommandQueue;
        }

        #endregion

        #region Destructors

        ~CalibrateAxisStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            switch (this.axisToCalibrate)
            {
                case Axis.Both:
                case Axis.Horizontal:
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.currentAxis = Axis.Vertical;
                    break;
            }

            this.CurrentState = new VoltageDisabledState(this, this.currentAxis);
        }

        #endregion
    }
}
