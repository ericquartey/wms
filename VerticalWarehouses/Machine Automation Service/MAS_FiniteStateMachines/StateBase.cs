using System;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public abstract class StateBase : IState
    {
        #region Fields

        protected IStateMachine ParentStateMachine;

        private bool disposed;

        #endregion

        #region Destructors

        ~StateBase()
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
        public abstract void ProcessCommandMessage(CommandMessage message);

        /// <inheritdoc />
        public abstract void ProcessFieldNotificationMessage(FieldNotificationMessage message);

        /// <inheritdoc />
        public abstract void ProcessNotificationMessage(NotificationMessage message);

        /// <inheritdoc />
        public abstract void Stop();

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
