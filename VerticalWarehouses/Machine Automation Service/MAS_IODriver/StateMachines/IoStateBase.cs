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
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public virtual string Type => this.GetType().ToString();

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void ProcessMessage(IoMessage message);

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
        }

        #endregion
    }
}
