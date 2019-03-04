using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Interface
{
    public interface IHomingStateMachine
    {
        #region Properties

        /// <summary>
        /// Get the <see cref="ICalibrateMessageData"/> message interface.
        /// </summary>
        ICalibrateMessageData CalibrateData { get; }

        /// <summary>
        /// Get the current state. Used for Unit Test
        /// </summary>
        IState GetState { get; }

        #endregion
    }
}
