namespace Ferretto.VW.MAS_FiniteStateMachines.Interface
{
    public interface IUpDownRepetitiveStateMachine
    {
        #region Properties

        /// <summary>
        /// Get the current state. Used for Unit Test
        /// </summary>
        IState GetState { get; }

        #endregion
    }
}
