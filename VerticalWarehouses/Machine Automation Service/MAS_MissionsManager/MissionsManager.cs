using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManager : AutomationBackgroundService
    {
        #region Fields

        private readonly AutoResetEvent bayNowServiceableResetEvent = new AutoResetEvent(false);

        private readonly IBaysManager baysManager;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoConfiguration;

        private readonly List<Mission> machineMissions = new List<Mission>();

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly ISetupNetworkDataLayer networkConfiguration;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        private int logCounterMissionManagement;

        #endregion

        #region Constructors

        public MissionsManager(
            IEventAggregator eventAggregator,
            ILogger<MissionsManager> logger,
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

            this.Logger.LogTrace("1:Method Start");

            this.baysManager = baysManager;
            this.generalInfoConfiguration = generalInfoConfiguration;
            this.networkConfiguration = networkConfiguration;
            this.machinesDataService = machinesDataService;
            this.missionsDataService = missionsDataService;

            this.missionManagementTask = new Task(() => this.HandleIncomingMissions());
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
                    await this.OnMissionCompleted(message.Data as IMissionCompletedMessageData);
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
                        bay.IsConnected
                        &&
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

            await this.SetupBays();
            await this.GetAllPendingMissions();

            this.missionManagementTask.Start();
        }

        private async Task OnMissionCompleted(IMissionCompletedMessageData missionCompletedData)
        {
            if (missionCompletedData == null)
            {
                throw new ArgumentNullException(nameof(missionCompletedData));
            }

            this.Logger.LogDebug($"MM NotificationCycle: MissionCompleted received");

            this.baysManager.Bays.Where(x => x.Id == missionCompletedData.BayId).First().Status = MAS_Utils.Enumerations.BayStatus.Idle;
            this.Logger.LogDebug($"MM NotificationCycle: Bay {missionCompletedData.BayId} status set to Available");
            await this.GetAllPendingMissions();
            this.bayNowServiceableResetEvent.Set();
        }

        private async Task OnNewMissionAvailable()
        {
            this.Logger.LogDebug($"MM NotificationCycle: MissionAdded received");
            await this.GetAllPendingMissions();
            this.newMissionArrivedResetEvent.Set();
            this.Logger.LogDebug($"MM NotificationCycle: MissionAdded completed");
        }

        #endregion
    }
}
