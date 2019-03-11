using System;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public abstract class InverterStateBase : IInverterState
    {
        #region Fields

        protected IInverterStateMachine parentStateMachine;

        private bool disposed = false;

        #endregion

        #region Destructors

        ~InverterStateBase()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public virtual string Type => this.GetType().ToString();

        #endregion

        #region Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void ProcessMessage(InverterMessage message);

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }

        #endregion
    }
}
