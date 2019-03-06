using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">The <see cref="IState"/> new state.</param>
        /// <param name="message">The <see cref="CommandMessage"/> to publish</param>
        void ChangeState(IState newState, CommandMessage message = null);

        /// <summary>
        /// Process an incoming command message.
        /// </summary>
        /// <param name="message">The <see cref="CommandMessage"/> to process.</param>
        void ProcessCommandMessage(CommandMessage message);

        /// <summary>
        /// Process an incoming notification message.
        /// </summary>
        /// <param name="message">The <see cref="NotificationMessage"/> to process.</param>
        void ProcessNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Publish a given command message.
        /// </summary>
        /// <param name="message">The <see cref="CommandMessage"/> to publish.</param>
        void PublishCommandMessage(CommandMessage message);

        /// <summary>
        /// Publish a given notification message.
        /// </summary>
        /// <param name="message">The <see cref="NotificationMessage"/> to publish.</param>
        void PublishNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Start.
        /// </summary>
        void Start();

        #endregion
    }
}
