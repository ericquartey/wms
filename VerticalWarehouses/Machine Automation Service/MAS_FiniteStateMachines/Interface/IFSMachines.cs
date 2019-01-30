namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IFSMachines
    {
        #region Methods

        void DoHoming();

        void DoVerticalHoming();

        #endregion Methods
    }

    // --------------------------
    // Interface IState
    public interface IState
    {
        #region Properties

        string Type { get; }

        #endregion Properties

        #region Methods

        void DoAction(IdOperation code);

        #endregion Methods
    }

    public interface IStateMachine
    {
        #region Methods

        void ChangeState(IState newState);

        void ExecuteOperation(IdOperation code);

        void Start();

        #endregion Methods
    }
}
