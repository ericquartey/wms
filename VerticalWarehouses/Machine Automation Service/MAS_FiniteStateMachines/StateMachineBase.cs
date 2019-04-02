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

        public bool OperationDone { get; set; }

        protected IState CurrentState { get; set; }

        protected IEventAggregator EventAggregator { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">The new state</param>
        /// <param name="message">A message to be published</param>
        public void ChangeState(IState newState, CommandMessage message = null)
        {
            this.CurrentState = newState;
            if (message != null) this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        /// <summary>
        /// On publishing the notification message by the state machine.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> message to be published.</param>
        public abstract void OnPublishNotification(NotificationMessage message);

        /// <summary>
        /// Process the command message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be parsed.</param>
        public abstract void ProcessCommandMessage(CommandMessage message);

        /// <summary>
        /// Process the notification message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be parsed.</param>
        public abstract void ProcessNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Publish a given Command message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be sent.</param>
        public void PublishCommandMessage(CommandMessage message)
        {
            this.EventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        /// <summary>
        /// Publish a given Notification message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be sent.</param>
        public void PublishNotificationMessage(NotificationMessage message)
        {
            this.EventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        /// <summary>
        /// Start the states machine.
        /// </summary>
        public abstract void Start();

        #endregion
    }
}
