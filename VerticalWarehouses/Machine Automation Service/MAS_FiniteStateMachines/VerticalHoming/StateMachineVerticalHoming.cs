using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming
{
    public class StateMachineVerticalHoming : IState, IStateMachine
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineVerticalHoming( INewInverterDriver driver, IEventAggregator eventAggregator )
        {
            this.driver = driver;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string Type => this.state.Type;

        #endregion

        #region Methods

        public void ChangeState( IState newState, Event_Message message = null )
        {
            this.state = newState;
        }

        public void NotifyMessage( Event_Message message )
        {
            throw new System.NotImplementedException();
        }

        public void PublishMessage( Event_Message message )
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            this.state = new VerticalHomingIdleState( this, this.driver, this.eventAggregator );
        }

        #endregion
    }
}
