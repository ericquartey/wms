using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineHoming : IState, IStateMachine
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineHoming( INewInverterDriver driver, INewRemoteIODriver remoteIODriver, IEventAggregator eventAggregator )
        {
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool HomingComplete { get; set; }

        public bool HorizontalHomingAlreadyDone { get; set; }

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
            this.HorizontalHomingAlreadyDone = false;
            this.HomingComplete = false;

            this.state = new HomingIdleState( this, this.driver, this.remoteIODriver, this.eventAggregator );
        }

        #endregion
    }
}
