using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService : BackgroundService
    {
        #region Fields

        private readonly IBaysDataService baysDataService;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataHubClient dataHubClient;

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly ILogger logger;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionDataService;

        private readonly BlockingConcurrentQueue<NotificationMessage> notificationQueue;

        private readonly Task notificationReceiveTask;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public AutomationService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<AutomationService> logger,
            IDataHubClient dataHubClient,
            IMachinesDataService machinesDataService,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IBaysDataService baysDataService,
            IMissionsDataService missionDataService,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (serviceScopeFactory == null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            logger.LogTrace("1:Method Start");

            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (installationHub == null)
            {
                throw new ArgumentNullException(nameof(installationHub));
            }

            if (dataHubClient == null)
            {
                throw new ArgumentNullException(nameof(dataHubClient));
            }

            if (machinesDataService == null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            if (operatorHub == null)
            {
                throw new ArgumentNullException(nameof(operatorHub));
            }

            if (baysDataService == null)
            {
                throw new ArgumentNullException(nameof(baysDataService));
            }

            if (missionDataService == null)
            {
                throw new ArgumentNullException(nameof(missionDataService));
            }

            this.eventAggregator = eventAggregator;
            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.machinesDataService = machinesDataService;
            this.operatorHub = operatorHub;
            this.baysDataService = baysDataService;
            this.missionDataService = missionDataService;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());

            this.InitializeMethodSubscriptions();

            this.dataHubClient.EntityChanged += this.OnWmsEntityChanged;
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.dataHubClient.ConnectAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();

#if DEBUG
                // simulate error
                await Task.Delay(20 * 1000);
                using (var scope = this.serviceScopeFactory.CreateScope())
                {
                    var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                    errorsProvider.RecordNew(MachineErrors.CradleNotCompletelyLoaded);
                }
#endif
            }
            catch (Exception ex)
            {
                throw new AutomationServiceException($"Exception: {ex.Message} while starting service threads.", ex);
            }
        }

        private void CommandReceiveTaskFunction()
        {
            do
            {
                try
                {
                    this.commandQueue.TryDequeue(Timeout.Infinite, this.stoppingToken, out var receivedMessage);
                    this.logger.LogTrace($"1:Dequeued Message:{receivedMessage.Type}:Destination{receivedMessage.Source}");
                    this.logger.LogTrace($"2:Waiting for process:{this.commandQueue.Count}");
                }
                catch (OperationCanceledException)
                {
                    this.logger.LogTrace("3:Method End - Operation Canceled");
                    return;
                }

                // TODO add here a switch block on receivedMessage.Type
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
                commandMessage => commandMessage.Destination == MessageActor.AutomationService || commandMessage.Destination == MessageActor.Any);

            this.logger.LogTrace("2:Notifications Subscription");
            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            notificationEvent.Subscribe(
                notificationMessage =>
                {
                    this.notificationQueue.Enqueue(notificationMessage);
                },
                ThreadOption.PublisherThread,
                false,
                notificationMessage => notificationMessage.Destination == MessageActor.AutomationService || notificationMessage.Destination == MessageActor.Any);
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
                    case MessageType.SensorsChanged:
                        this.SensorsChangedMethod(receivedMessage);
                        break;

                    case MessageType.Homing:
                        this.HomingMethod(receivedMessage);
                        break;

                    case MessageType.SwitchAxis:
                        this.SwitchAxisMethod(receivedMessage);
                        break;

                    case MessageType.ShutterPositioning:
                        this.ShutterPositioningMethod(receivedMessage);
                        break;

                    case MessageType.CalibrateAxis:
                        //case MessageType.DataLayerReady:
                        //case MessageType.IOPowerUp:
                        this.CalibrateAxisMethod(receivedMessage);
                        break;

                    case MessageType.CurrentPosition:
                        this.CurrentPositionMethod(receivedMessage);
                        break;

                    case MessageType.ShutterTestStatusChanged:
                        this.ShutterControlMethod(receivedMessage);
                        break;

                    case MessageType.Positioning:
                        this.PositioningMethod(receivedMessage);
                        break;

                    case MessageType.FsmException:
                    case MessageType.InverterException:
                    case MessageType.IoDriverException:
                    case MessageType.DlException:
                    case MessageType.WebApiException:

                        break;

                    case MessageType.ResolutionCalibration:
                        this.ResolutionCalibrationMethod(receivedMessage);
                        break;

                    case MessageType.ExecuteMission:
                        if (receivedMessage.Data is INewMissionOperationAvailable data)
                        {
                            await this.OnNewMissionOperationAvailable(data);
                            this.logger.LogDebug($"AS-AS NotificationCycle: ExecuteMission id: {data.MissionId}, mission quantity: {data.PendingMissionsCount}");
                        }
                        break;

                    case MessageType.BayOperationalStatusChanged:
                        this.logger.LogDebug($"AS NotificationCycle: BayConnected received");
                        this.OnBayConnected(receivedMessage.Data as IBayOperationalStatusChangedMessageData);
                        break;

                    case MessageType.DataLayerReady:
                        this.OnDataLayerReady();
                        break;

                    case MessageType.ErrorStatusChanged:
                        this.OnErrorStatusChanged(receivedMessage.Data as IErrorStatusMessageData);
                        break;

                    case MessageType.InverterStatusWord:
                        this.InverterStatusWordMethod(receivedMessage);
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("9:Method End");
        }

        private void OnDataLayerReady()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var baysConfigurationProvider = scope.ServiceProvider.GetRequiredService<IBaysConfigurationProvider>();

                baysConfigurationProvider.LoadFromConfiguration();
            }
        }

        #endregion
    }
}
