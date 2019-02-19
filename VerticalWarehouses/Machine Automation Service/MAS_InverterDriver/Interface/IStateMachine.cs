namespace Ferretto.VW.MAS_InverterDriver
{
    public interface IStateMachine
    {
        #region Methods

        void ChangeState(IState newState);

        #endregion
    }
}
