using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.NewHoming
{
    public class HomingStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ICalibrateMessageData calibrateMessageData;

        #endregion

        #region Constructors

        public HomingStateMachine(IEventAggregator eventAggregator, ICalibrateMessageData calibrateMessageData)
            : base(eventAggregator)
        {
            this.calibrateMessageData = calibrateMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.CurrentState = new HomingStartState(this);
        }

        #endregion
    }
}
