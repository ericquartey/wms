using System;
using Ferretto.VW.MAS_IODriver.Interface;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_IODriver.StateMachines
{
    public abstract class IoStateBase : IIoState
    {
        #region Fields

        protected IIoStateMachine ParentStateMachine;

        private bool disposed;

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

        //public abstract void ProcessMessage(IoMessage message);
        public abstract void ProcessMessage(IoSHDMessage message);

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
