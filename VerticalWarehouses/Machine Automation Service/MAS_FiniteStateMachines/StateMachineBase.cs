using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public abstract class StateMachineBase : IStateMachine
    {
        #region Fields

        protected BlockingConcurrentQueue<CommandMessage> stateMachineCommandQueue;

        #endregion

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

        public void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState?.SendCommandMessage(message);
        }

        public void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState?.SendNotificationMessage(message);
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
