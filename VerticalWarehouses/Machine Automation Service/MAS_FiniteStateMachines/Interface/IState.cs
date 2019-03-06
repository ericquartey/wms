using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IState
    {
        #region Properties

        /// <summary>
        /// Get the type of state (string description).
        /// </summary>
        string Type { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Send a command message.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> is published from state</param>
        void SendCommandMessage(CommandMessage message);

        /// <summary>
        /// Send a notification message.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> is published from state.</param>
        void SendNotificationMessage(NotificationMessage message);

        #endregion
    }
}
