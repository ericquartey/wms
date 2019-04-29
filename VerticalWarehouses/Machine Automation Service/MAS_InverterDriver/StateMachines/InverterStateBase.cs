using System;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public abstract class InverterStateBase : IInverterState
    {
        #region Fields

        protected IInverterStateMachine ParentStateMachine;

        private bool disposed;

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

        /// <inheritdoc />
        public abstract void Stop();

        /// <inheritdoc />
        public abstract bool ValidateCommandResponse(InverterMessage message);

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
