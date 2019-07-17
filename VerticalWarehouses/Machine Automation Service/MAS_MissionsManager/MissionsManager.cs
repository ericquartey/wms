using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_MissionsManager
{
    public partial class MissionsManager : BackgroundService
    {
        #region Fields

        private readonly IBaysManager baysManager;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private AutoResetEvent bayNowServiceableResetEvent;

        private bool disposed;

        private IGeneralInfo generalInfo;

        private int lastServedBay;

        private int logCounterMissionManagement;

        private List<Mission> machineMissions;

        private IMissionsDataService missionsDataService;

        private AutoResetEvent newMissionArrivedResetEvent;

        private ISetupNetwork setupNetwork;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public MissionsManager(
            IEventAggregator eventAggregator,
            ILogger<MissionsManager> logger,
            IGeneralInfo generalInfo,
            IBaysManager baysManager,
            ISetupNetwork setupNetwork,
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService)
        {
            logger.LogTrace("1:Method Start");

            this.eventAggregator = eventAggregator;
            this.baysManager = baysManager;
            this.logger = logger;
            this.generalInfo = generalInfo;
            this.setupNetwork = setupNetwork;
            this.machinesDataService = machinesDataService;
            this.missionsDataService = missionsDataService;

            this.machineMissions = new List<Mission>();

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());
            this.missionManagementTask = new Task(() => this.MissionManagementTaskFunction());

            this.bayNowServiceableResetEvent = new AutoResetEvent(false);
            this.newMissionArrivedResetEvent = new AutoResetEvent(false);

            this.InitializeMethodSubscriptions();
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new MissionsManagerException($"Exception: {ex.Message} while starting service threads", ex);
            }
        }

        private void CommandReceiveTaskFunction()
        {
            do
            {
                CommandMessage receivedMessage;
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);
                    this.logger.LogTrace($"1:Dequeued Message:{receivedMessage.Type}:Destination{receivedMessage.Source}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogTrace("2:Method End - Operation Canceled");
                    return;
                }

                switch (receivedMessage.Type)
                {
                    // TODO
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private void InitializeMethodSubscriptions()
        {
            this.logger.LogTrace("1:Commands Subscription");
            var commandEvent = this.eventAggregator.GetEvent<CommandEvent>();
            commandEvent.Subscribe(
                commandMessage =>
                {
                    this.commandQueue.Enqueue(commandMessage);
                },
                ThreadOption.PublisherThread,
                false,
                commandMessage =>
                commandMessage.Destination == MessageActor.MissionsManager ||
                commandMessage.Destination == MessageActor.Any);

            this.logger.LogTrace("2:Notifications Subscription");
            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                notificationMessage =>
                {
                    this.notificationQueue.Enqueue(notificationMessage);
                },
                ThreadOption.PublisherThread,
                false,
                notificationMessage =>
                notificationMessage.Destination == MessageActor.MissionsManager ||
                notificationMessage.Destination == MessageActor.Any);
        }

        private async void MissionManagementTaskFunction()
        {
            this.logger.LogTrace("1:Method Start");

            do
            {
                this.logger.LogDebug($"MM MissionManagementCycle: Start iteration #{this.logCounterMissionManagement}");
                if (this.IsAnyBayServiceable())
                {
                    this.logger.LogDebug($"MM MissionManagementCycle: Iteration #{this.logCounterMissionManagement}: serviceable bay present");
                    if (this.IsAnyMissionExecutable())
                    {
                        this.logger.LogDebug($"MM MissionManagementCycle: Iteration #{this.logCounterMissionManagement}: executable mission present");
                        await this.ChooseAndExecuteMission();
                    }
                    else
                    {
                        this.logger.LogDebug($"MM MissionManagementCycle: End iteration #{this.logCounterMissionManagement++}: NO executable mission present");
                        WaitHandle.WaitAny(new WaitHandle[] { this.bayNowServiceableResetEvent, this.newMissionArrivedResetEvent, this.stoppingToken.WaitHandle });
                    }
                }
                else
                {
                    this.logger.LogDebug($"MM MissionManagementCycle: End iteration #{this.logCounterMissionManagement++}: NO serviceable bay present");
                    WaitHandle.WaitAny(new WaitHandle[] { this.bayNowServiceableResetEvent, this.newMissionArrivedResetEvent, this.stoppingToken.WaitHandle });
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);
        }

        private async void NotificationReceiveTaskFunction()
        {
            do
            {
                NotificationMessage receivedMessage;
                try
                {
                    this.notificationQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out receivedMessage);

                    this.logger.LogTrace($"1:Notification received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}, status: {receivedMessage.Status}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogDebug("2:Method End - Operation Canceled");

                    return;
                }

                switch (receivedMessage.Type)
                {
                    case MessageType.MissionCompleted:
                        this.logger.LogDebug($"MM NotificationCycle: MissionCompleted received");
                        if (receivedMessage.Data is IMissionCompletedMessageData missionCompletedData)
                        {
                            this.baysManager.Bays.Where(x => x.Id == missionCompletedData.BayId).First().Status = MAS_Utils.Enumerations.BayStatus.Available;
                            this.logger.LogDebug($"MM NotificationCycle: Bay {missionCompletedData.BayId} status set to Available");
                            await this.DistributeMissions();
                            this.bayNowServiceableResetEvent.Set();
                        }
                        break;

                    case MessageType.BayConnected:
                        this.logger.LogDebug($"MM NotificationCycle: BayConnected received");
                        if (receivedMessage.Data is IBayConnectedMessageData bayConnectedData)
                        {
                            this.bayNowServiceableResetEvent.Set();
                        }
                        break;

                    case MessageType.MissionAdded:
                        this.logger.LogDebug($"MM NotificationCycle: MissionAdded received");
                        await this.DistributeMissions();
                        this.newMissionArrivedResetEvent.Set();
                        this.logger.LogDebug($"MM NotificationCycle: MissionAdded completed");
                        break;

                    case MessageType.DataLayerReady:
                        this.logger.LogDebug($"MM NotificationCycle: DataLayerReady received");
                        await this.InitializeBays();
                        await this.DistributeMissions();
                        this.missionManagementTask.Start();
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);

            return;
        }

        #endregion
    }
}
