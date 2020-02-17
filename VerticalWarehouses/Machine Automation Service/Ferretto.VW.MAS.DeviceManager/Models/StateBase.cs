using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.DeviceManager
{
    internal abstract class StateBase : IState
    {
        #region Fields

        public const int SENSOR_UPDATE_FAST = 50;

        public const int SENSOR_UPDATE_SLOW = 500;

        #endregion

        #region Constructors

        protected StateBase(
            IStateMachine parentStateMachine,
            ILogger logger)
        {
            this.ParentStateMachine = parentStateMachine;
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.Logger.LogTrace($"State '{this.GetType().Name}' initialized.");
        }

        #endregion

        #region Properties

        public ILogger Logger { get; }

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

        /// <param name="reason"></param>
        /// <inheritdoc />
        public abstract void Stop(StopRequestReason reason);

        #endregion
    }
}
