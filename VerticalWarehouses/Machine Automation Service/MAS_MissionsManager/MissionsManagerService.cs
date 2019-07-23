using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService : AutomationBackgroundService
    {
        #region Fields

        private readonly AutoResetEvent bayNowServiceableResetEvent = new AutoResetEvent(false);

        private readonly IBaysManager baysManager;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoConfiguration;

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly ISetupNetworkDataLayer networkConfiguration;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        private int logCounterMissionManagement;

        #endregion

        #region Constructors

        public MissionsManagerService(
            IEventAggregator eventAggregator,
            ILogger<MissionsManagerService> logger,
            IGeneralInfoConfigurationDataLayer generalInfoConfiguration,
            IBaysManager baysManager,
            ISetupNetworkDataLayer networkConfiguration,
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService)
            : base(eventAggregator, logger)
        {
            if (generalInfoConfiguration == null)
            {
                throw new ArgumentNullException(nameof(generalInfoConfiguration));
            }

            if (baysManager == null)
            {
                throw new ArgumentNullException(nameof(baysManager));
            }

            if (networkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(networkConfiguration));
            }

            if (machinesDataService == null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            if (missionsDataService == null)
            {
                throw new ArgumentNullException(nameof(missionsDataService));
            }

            this.baysManager = baysManager;
            this.generalInfoConfiguration = generalInfoConfiguration;
            this.networkConfiguration = networkConfiguration;
            this.machinesDataService = machinesDataService;
            this.missionsDataService = missionsDataService;

            this.missionManagementTask = new Task(() => this.HandleIncomingMissions());

            this.Logger.LogTrace("1:Mission manager initialised.");
        }

        #endregion

        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Destination == MessageActor.MissionsManager ||
                command.Destination == MessageActor.Any;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return notification.Destination == MessageActor.MissionsManager ||
                  notification.Destination == MessageActor.Any;
        }

        protected override Task OnCommandReceivedAsync(CommandMessage command)
        {
            // do nothing
            return Task.CompletedTask;
        }

        protected override async Task OnNotificationReceivedAsync(NotificationMessage message)
        {
            switch (message.Type)
            {
                case MessageType.MissionCompleted:
                    await this.OnMissionOperationCompleted(message.Data as IMissionOperationCompletedMessageData);
                    break;

                case MessageType.BayConnected:
                    this.OnBayConnected();
                    break;

                case MessageType.MissionAdded:
                    await this.OnNewMissionAvailable();
                    break;

                case MessageType.DataLayerReady:
                    await this.OnDataLayerReady();
                    break;
            }
        }

        private void HandleIncomingMissions()
        {
            do
            {
                this.Logger.LogDebug($"MM MissionManagementCycle: Start iteration #{this.logCounterMissionManagement}");

                var hasActiveBaysAndPendingMissions =
                    this.baysManager.Bays.Any(bay =>
                        bay.Status == BayStatus.Idle
                        &&
                        bay.PendingMissions.Any());

                if (hasActiveBaysAndPendingMissions)
                {
                    this.Logger.LogDebug($"MM MissionManagementCycle: Iteration #{this.logCounterMissionManagement}: serviceable bay present & executable mission present");
                    this.ChooseAndExecuteMission();
                }
                else
                {
                    this.Logger.LogDebug($"MM MissionManagementCycle: End iteration #{this.logCounterMissionManagement++}: NO serviceable bay present");
                    WaitHandle.WaitAny(new WaitHandle[] { this.bayNowServiceableResetEvent, this.newMissionArrivedResetEvent, this.StoppingToken.WaitHandle });
                }
            }
            while (!this.StoppingToken.IsCancellationRequested);
        }

        private void OnBayConnected()
        {
            this.Logger.LogDebug($"MM NotificationCycle: BayConnected received.");

            this.bayNowServiceableResetEvent.Set();
        }

        private async Task OnDataLayerReady()
        {
            this.Logger.LogDebug($"MM NotificationCycle: DataLayerReady received.");

            await this.baysManager.SetupBaysAsync();
            await this.RefreshPendingMissionsQueue();

            this.missionManagementTask.Start();
        }

        private async Task OnMissionOperationCompleted(IMissionOperationCompletedMessageData e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.Logger.LogDebug($"MM NotificationCycle: MissionCompleted received");

            var bay = this.baysManager.Bays
                .Where(b => b.CurrentMissionOperation != null)
                .SingleOrDefault(b => b.CurrentMissionOperation.Id == e.MissionOperationId);

            if (bay != null)
            {
                bay.Status = BayStatus.Idle;
                bay.CurrentMissionOperation.Status = MissionOperationStatus.Completed;
                bay.CurrentMissionOperation = null;
            }

            this.Logger.LogDebug($"MM NotificationCycle: Bay {bay.Id} status set to Available");

            await this.RefreshPendingMissionsQueue();

            this.bayNowServiceableResetEvent.Set();
        }

        private async Task OnNewMissionAvailable()
        {
            this.Logger.LogDebug($"MM NotificationCycle: MissionAdded received");

            await this.RefreshPendingMissionsQueue();
            this.newMissionArrivedResetEvent.Set();

            this.Logger.LogDebug($"MM NotificationCycle: MissionAdded completed");
        }

        #endregion
    }
}
