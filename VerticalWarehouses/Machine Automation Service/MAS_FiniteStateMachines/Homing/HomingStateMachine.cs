using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingStateMachine : StateMachineBase
    {
        #region Fields

        private readonly Axis calibrateAxis;

        private readonly ICalibrateMessageData calibrateMessageData;

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

        #endregion

        #region Methods

        public override void Start()
        {
            this.CurrentState = new HomingStartState(this);
        }

        #endregion
    }
}
