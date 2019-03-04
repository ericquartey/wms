namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public abstract class IoStateBase : IIoState
    {
        #region Fields

        protected IIoStateMachine parentStateMachine;

        #endregion

        #region Properties

        public virtual string Type => this.GetType().ToString();

        #endregion

        #region Methods

        public abstract void ProcessMessage(IoMessage message);

        #endregion
    }
}
