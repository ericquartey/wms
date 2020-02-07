using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MachineManager
{
    internal partial class MachineManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly object syncMachineObject = new object();

        private readonly object syncObject = new object();

        private bool isDisposed;

        #endregion

        #region Constructors

        public MachineManagerService(
            IEventAggregator eventAggregator,
            ILogger<MachineManagerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.isDataLayerReady = false;
            this.Logger.LogTrace("Mission manager initialized.");
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();

            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }

        protected override void NotifyCommandError(CommandMessage notificationData)
        {
            this.Logger.LogDebug($"Notifying Mission Manager service command error");

            var msg = new NotificationMessage(
                notificationData?.Data,
                "MM Command Error",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.MachineManagerException,
                notificationData?.RequestingBay ?? BayNumber.None,
                notificationData?.TargetBay ?? BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        protected override void NotifyError(NotificationMessage notificationData)
        {
            this.Logger.LogDebug($"Notifying Mission Manager service notification error");

            var msg = new NotificationMessage(
                notificationData?.Data,
                "MM Notification Error",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.MachineManagerException,
                notificationData?.RequestingBay ?? BayNumber.None,
                notificationData?.TargetBay ?? BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        #endregion
    }
}
