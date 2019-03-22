using System;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;

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

        /// <inheritdoc />
        public abstract bool ProcessMessage(InverterMessage message);

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
