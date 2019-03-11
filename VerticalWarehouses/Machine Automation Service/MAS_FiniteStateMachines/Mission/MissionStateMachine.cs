using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Mission
{
    public class MissionStateMachine : StateMachineBase
    {
        #region Constructors

        public MissionStateMachine(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.CurrentState = new MissionStartState(this);
        }

        #endregion
    }
}
