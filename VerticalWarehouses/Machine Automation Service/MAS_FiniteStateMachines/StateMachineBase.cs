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
            this.EventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        protected IState CurrentState { get; set; }

        protected IEventAggregator EventAggregator { get; }

        #endregion

        #region Methods

        public void ChangeState(IState newState, CommandMessage message = null)
        {
            this.CurrentState = newState;
            if (message != null) this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        public void NotifyMessage(CommandMessage message)
        {
            //TODO to remove
            this.CurrentState?.SendCommandMessage(message);
        }

        public void PublishCommandMessage(CommandMessage message)
        {
            this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        public void PublishNotificationMessage(NotificationMessage message)
        {
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        public abstract void Start();

        #endregion
    }
}
