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

        void MakeOperation();

        void SendCommandMessage(CommandMessage message);

        void SendNotificationMessage(NotificationMessage message);

        void Stop();

        #endregion
    }
}
