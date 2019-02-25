namespace Ferretto.VW.InverterDriver.StateMachines
{
    public abstract class InverterStateBase : IInverterState
    {
        #region Fields

        protected IInverterStateMachine parentStateMachine;

        #endregion

        #region Properties

        public string Type => "InverterBaseState";

        #endregion

        #region Methods

        public abstract void NotifyMessage( InverterMessage message );

        #endregion
    }
}
