using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.InverterDriver.StateMachines
{
    internal abstract class InverterStateBase : IInverterState
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public InverterStateBase(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
        {
            if (inverterStatus is null)
            {
                throw new ArgumentNullException(nameof(inverterStatus));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.InverterStatus = inverterStatus;

            this.ParentStateMachine = parentStateMachine;

            this.Logger = logger;

            this.Logger.LogTrace($"Inverter state '{this.GetType().Name}' initialized.");
        }

        #endregion

        #region Properties

        public virtual string Type => this.GetType().ToString();

        protected IInverterStatusBase InverterStatus { get; }

        protected ILogger Logger { get; }

        protected IInverterStateMachine ParentStateMachine { get; }

        #endregion

        #region Methods

        public virtual void Continue()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public abstract void Start();

        /// <inheritdoc />
        public abstract void Stop();

        /// <inheritdoc />
        public abstract bool ValidateCommandMessage(InverterMessage message);

        /// <inheritdoc />
        public abstract bool ValidateCommandResponse(InverterMessage message);

        protected void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.OnDisposing();
            }

            this.disposed = true;
        }

        protected virtual void OnDisposing()
        {
            // Do nothing here.
            // Derived classes can override this method to dispose of specific resources.
        }

        #endregion
    }
}
