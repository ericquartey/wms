namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IStateMachine
    {
        #region Methods

        void ChangeState(IState newState);

        void ExecuteOperation(IdOperation code);

        void Start();

        #endregion Methods
    }
}
