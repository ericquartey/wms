using System;

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public abstract class IoStateBase : IIoState
    {
        #region Fields

        protected IIoStateMachine parentStateMachine;

        private bool disposed = false;

        #endregion

        #region Destructors

        ~IoStateBase()
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

        public abstract void ProcessMessage(IoMessage message);

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
