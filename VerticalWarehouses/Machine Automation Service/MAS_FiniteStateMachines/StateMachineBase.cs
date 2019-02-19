using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public abstract class StateMachineBase : IStateMachine
    {
        #region Constructors

        protected StateMachineBase(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        protected IState currentState { get; set; }

        protected IEventAggregator eventAggregator { get; }

        #endregion

        #region Methods

        public void ChangeState(IState newState, Event_Message message = null)
        {
            this.currentState = newState;
            if (message != null)
            {
                this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
            }
        }

        public void NotifyMessage(Event_Message message)
        {
            this.currentState.NotifyMessage(message);
        }

        public void PublishMessage(Event_Message message)
        {
            this.eventAggregator.GetEvent<MachineAutomationService_Event>().Publish(message);
        }

        public abstract void Start();

        #endregion
    }
}
