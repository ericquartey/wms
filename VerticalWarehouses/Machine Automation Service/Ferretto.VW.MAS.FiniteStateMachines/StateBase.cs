using System;
using Ferretto.VW.CommonUtils.Messages;
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

        #endregion

        #region Constructors

        public StateBase(
            IStateMachine parentStateMachine,
            ILogger logger)
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

        #region Properties

        public virtual string Type => this.GetType().ToString();

        protected ILogger Logger { get; }

        protected IStateMachine ParentStateMachine { get; }

        #endregion

        #region Methods

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

        #endregion
    }
}
