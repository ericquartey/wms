namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Methods

        void ChangeState(IState newState);

        void Start();

        #endregion
    }
}
