using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Interface
{
    public interface IPositioningStateMachine
    {
        #region Properties

        /// <summary>
        /// Get the current state. Used for Unit Test
        /// </summary>
        IState GetState { get; }

        /// <summary>
        /// Get the <see cref="IPositioningMessageData"/> message interface.
        /// </summary>
        IPositioningMessageData PositioningData { get; }

        #endregion
    }
}
