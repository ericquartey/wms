using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines
{
    internal abstract class StateBase : IState
    {
        #region Fields

        public const int SENSOR_UPDATE_FAST = 50;

        public const int SENSOR_UPDATE_SLOW = 500;

        private bool disposed;

        #endregion

        #region Constructors

        protected StateBase(
            IStateMachine parentStateMachine,
            ILogger<FiniteStateMachines> logger)
        {
            this.ParentStateMachine = parentStateMachine;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Logger.LogTrace($"State '{this.GetType().Name}' initialized.");
        }

        #endregion

        #region Destructors

        ~StateBase()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        public ILogger<FiniteStateMachines> Logger { get; }

        public virtual string Type => this.GetType().ToString();

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

        /// <param name="reason"></param>
        /// <inheritdoc />
        public abstract void Stop(StopRequestReason reason);

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
