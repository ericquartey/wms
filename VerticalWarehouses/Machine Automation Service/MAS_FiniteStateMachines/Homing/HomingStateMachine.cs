using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStateMachine : StateMachineBase, IHomingStateMachine
    {
        #region Fields

        private readonly Axis calibrateAxis;

        private readonly ICalibrateMessageData calibrateMessageData;

        private Axis currentAxis;

        #endregion

        #region Constructors

        public HomingStateMachine(IEventAggregator eventAggregator, ICalibrateMessageData calibrateMessageData)
            : base(eventAggregator)
        {
            this.calibrateMessageData = calibrateMessageData;
            this.calibrateAxis = calibrateMessageData.AxisToCalibrate;
        }

        #endregion

        #region Properties

        public ICalibrateMessageData CalibrateData => this.calibrateMessageData;

        public IState GetState => this.CurrentState;

        public bool IsStopRequested { get; set; }

        public int NMaxSteps { get; private set; }

        public int NumberOfExecutedSteps { get; set; }

        #endregion

        #region Methods

        public void ChangeAxis(Axis axisToCalibrate)
        {
            this.currentAxis = axisToCalibrate;
        }

        public override void Start()
        {
            switch (this.calibrateAxis)
            {
                case Axis.Both:
                    {
                        this.NMaxSteps = 3;
                        this.NumberOfExecutedSteps = 0;
                        this.currentAxis = Axis.Horizontal;
                        break;
                    }
                case Axis.Horizontal:
                    {
                        this.NMaxSteps = 1;
                        this.NumberOfExecutedSteps = 0;
                        this.currentAxis = Axis.Horizontal;
                        break;
                    }

                case Axis.Vertical:
                    {
                        this.NMaxSteps = 1;
                        this.NumberOfExecutedSteps = 0;
                        this.currentAxis = Axis.Vertical;
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            this.CurrentState = new HomingStartState(this, this.currentAxis);
        }

        #endregion
    }
}
