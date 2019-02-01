namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IState
    {
        #region Properties

        string Type { get; }

        #endregion Properties

        #region Methods

        void DoAction(IdOperation code);

        #endregion Methods
    }
}
