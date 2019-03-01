using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Calibrate;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public class CalibrateStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private Axis currentAxis;

        #endregion

        #region Constructors

        //TODO remove priority queue
        public CalibrateStateMachine(Axis axisToCalibrate,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            BlockingConcurrentQueue<InverterMessage> priorityInverterCommandQueue)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.inverterCommandQueue = inverterCommandQueue;
            //this.priorityInverterCommandQueue = priorityInverterCommandQueue;
        }

        #endregion

        #region Methods

        public override void ChangeState(IInverterState newState)
        {
            if (newState is EndState)
                if (this.axisToCalibrate == Axis.Both && this.currentAxis == Axis.Horizontal)
                {
                    this.currentAxis = Axis.Vertical;
                    base.ChangeState(new VoltageDisabledState(this, this.currentAxis));
                    return;
                }

            base.ChangeState(newState);
        }

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
