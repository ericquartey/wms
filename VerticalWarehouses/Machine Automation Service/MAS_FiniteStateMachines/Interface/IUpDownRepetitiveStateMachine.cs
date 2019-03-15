using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Interface
{
    public interface IUpDownRepetitiveStateMachine
    {
        #region Properties

        /// <summary>
        /// Get the current state. Used for Unit Test
        /// </summary>
        IState GetState { get; }

        /// <summary>
        /// <c>True</c> if a request stop has been done.
        /// </summary>
        bool IsStopRequested { get; set; }

        /// <summary>
        /// Retrieve the number of completed cycles.
        /// </summary>
        int NumberOfCompletedCycles { get; set; }

        /// <summary>
        /// Get the <see cref="IUpDownRepetitiveMessageData"/> message interface.
        /// </summary>
        IUpDownRepetitiveMessageData UpDownRepetitiveData { get; }

        #endregion
    }
}
