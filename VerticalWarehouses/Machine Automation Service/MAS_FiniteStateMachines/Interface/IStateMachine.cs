using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Properties

        bool OperationDone { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">The new state</param>
        /// <param name="message">A message to be published</param>
        void ChangeState(IState newState, CommandMessage message = null);

        /// <summary>
        /// On publishing the notification message by the state machine.
        /// The notification message is kept, it is handled and after it is published.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> message to be handled and published.</param>
        void OnPublishNotification(NotificationMessage message);

        /// <summary>
        /// Process the command message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be parsed.</param>
        void ProcessCommandMessage(CommandMessage message);

        /// <summary>
        /// Process the notification message incoming to the Finite State Machines.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be parsed.</param>
        void ProcessNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Publish a given Command message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> command message to be sent.</param>
        void PublishCommandMessage(CommandMessage message);

        /// <summary>
        /// Publish a given Notification message via EventAggregator.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be sent.</param>
        void PublishNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Start the states machine.
        /// </summary>
        void Start();

        #endregion
    }
}
