using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Utilities;
using Prism.Events;

namespace Ferretto.VW.InverterDriver.StateMachines.Calibrate
{
    public class CalibrateStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private int calibrationStep;

        #endregion

        #region Constructors

        public CalibrateStateMachine(Axis axisToCalibrate,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.inverterCommandQueue = inverterCommandQueue;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public override void ChangeState(IInverterState newState)
        {
            if (newState is EndState)
                if (this.axisToCalibrate == Axis.Both)
                {
                    switch (this.calibrationStep)
                    {
                        case 0:
                            this.calibrationStep++;
                            base.ChangeState(new VoltageDisabledState(this, Axis.Vertical));
                            return;

                        case 1:
                            this.calibrationStep++;
                            base.ChangeState(new VoltageDisabledState(this, Axis.Horizontal));
                            return;
                    }
                }

            base.ChangeState(newState);
        }

        public override void Start()
        {
            switch (this.axisToCalibrate)
            {
                case Axis.Both:
                case Axis.Horizontal:
                    this.CurrentState = new VoltageDisabledState(this, Axis.Horizontal);
                    break;

                case Axis.Vertical:
                    this.CurrentState = new VoltageDisabledState(this, Axis.Vertical);
                    break;
            }
        }

        #endregion
    }
}
