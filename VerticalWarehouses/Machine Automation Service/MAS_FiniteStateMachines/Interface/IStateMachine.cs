namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Methods

        /// <summary>
        /// Change the state.
        /// </summary>
        /// <param name="newState">The new state.</param>
        void ChangeState(IState newState);

        /// <summary>
        /// Start operation.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop operation.
        /// </summary>
        void Stop();

        #endregion
    }
}
