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

            this.Logger.LogTrace("State '{0}' initialized.", this.GetType().Name);
        }

        #endregion

        #region Properties

        protected ILogger Logger { get; }

        protected IStateMachine ParentStateMachine { get; }

        #endregion

        #region Methods

        public abstract void ProcessCommandMessage(CommandMessage message);

        public abstract void ProcessFieldNotificationMessage(FieldNotificationMessage message);

        public abstract void ProcessNotificationMessage(NotificationMessage message);

        public abstract void Start();

        public abstract void Stop(StopRequestReason reason);

        #endregion
    }
}
