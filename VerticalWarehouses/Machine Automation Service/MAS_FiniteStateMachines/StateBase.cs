using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public abstract class StateBase : IState
    {
        #region Fields

        private bool disposed;

        #endregion

        #region Constructors

        public StateBase(IStateMachine parentStateMachine, ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.ParentStateMachine = parentStateMachine;
            this.Logger = logger;

            this.Logger.LogTrace($"State '{this.GetType().Name}' initialised.");
        }

        #endregion

        #region Destructors

        ~StateBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public virtual string Type => this.GetType().ToString();

        protected ILogger Logger { get; }

        protected IStateMachine ParentStateMachine { get; }

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
        public abstract void Start();

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
