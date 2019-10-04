using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.BackgroundServices
{
    internal partial class MissionsManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly AutoResetEvent bayStatusChangedEvent = new AutoResetEvent(false);

        private readonly IConfiguration configuration;

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly IMissionsProvider missionsProvider;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        private readonly IServiceScope serviceScope;

        private bool isDisposed;

        #endregion

        #region Constructors

        public MissionsManagerService(
            IEventAggregator eventAggregator,
            ILogger<MissionsManagerService> logger,
            IMachinesDataService machinesDataService,
            IMissionsProvider missionsProvider,
            IMissionsDataService missionsDataService,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.missionsProvider = missionsProvider ?? throw new ArgumentNullException(nameof(missionsProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.serviceScope = serviceScopeFactory.CreateScope();

            this.missionManagementTask = new Task(async () => await this.ScheduleMissionsOnBaysAsync());

            this.Logger.LogTrace("Mission manager initialized.");
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();

            if (!this.isDisposed)
            {
                this.serviceScope.Dispose();

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
                MessageActor.MissionsManager,
                MessageType.MissionManagerException,
                notificationData?.RequestingBay ?? BayNumber.None,
                notificationData?.TargetBay ?? BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        protected override void NotifyError(NotificationMessage notificationData)
        {
            this.Logger.LogDebug($"Notifying Mission Manager service notification error");

            var msg = new NotificationMessage(
                notificationData?.Data,
                "MM Notification Error",
                MessageActor.Any,
                MessageActor.MissionsManager,
                MessageType.MissionManagerException,
                notificationData?.RequestingBay ?? BayNumber.None,
                notificationData?.TargetBay ?? BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        #endregion
    }
}
