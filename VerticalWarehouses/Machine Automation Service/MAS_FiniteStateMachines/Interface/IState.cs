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
        /// Make the operation required by the current state.
        /// </summary>
        void MakeOperation();

        void NotifyMessage(Event_Message message);

        /// <summary>
        /// Stop operation for the state.
        /// </summary>
        void Stop();

        #endregion
    }
}
