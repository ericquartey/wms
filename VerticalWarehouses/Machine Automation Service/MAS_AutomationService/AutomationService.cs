using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Exceptions;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Utilities;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService : BackgroundService
    {
        #region Fields

        private readonly IBaysManager baysManager;

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

        private bool disposed;

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
            IMissionsDataService missionDataService,
            IBaysManager baysManager)
        {
            logger.LogTrace("1:Method Start");
            this.eventAggregator = eventAggregator;
            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.machinesDataService = machinesDataService;
            this.operatorHub = operatorHub;
            this.missionDataService = missionDataService;
            this.baysManager = baysManager;

            this.logger = logger;

            this.commandQueue = new BlockingConcurrentQueue<CommandMessage>();
            this.notificationQueue = new BlockingConcurrentQueue<NotificationMessage>();

            this.commandReceiveTask = new Task(() => this.CommandReceiveTaskFunction());
            this.notificationReceiveTask = new Task(() => this.NotificationReceiveTaskFunction());

            this.InitializeMethodSubscriptions();
            this.dataHubClient.ConnectAsync();

            this.dataHubClient.ConnectionStatusChanged += this.DataHubClient_ConnectionStatusChanged;
            this.dataHubClient.EntityChanged += this.DataHubClient_EntityChanged;
        }

        #endregion

        #region Destructors

        ~AutomationService()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public void Dispose(bool disposing)
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.stoppingToken = stoppingToken;

            try
            {
                this.commandReceiveTask.Start();
                this.notificationReceiveTask.Start();
            }
            catch (Exception ex)
            {
                throw new AutomationServiceException($"Exception: {ex.Message} while starting service threads.", ex);
            }

            return Task.CompletedTask;
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

                    case MessageType.ShutterControl:
                        this.ShutterControlMethod(receivedMessage);
                        break;

                    case MessageType.Positioning:
                        this.PositioningMethod(receivedMessage);
                        break;

                    case MessageType.FSMException:
                    case MessageType.InverterException:
                    case MessageType.IoDriverException:
                    case MessageType.DLException:
                    case MessageType.WebApiException:
                        this.ExceptionHandlerMethod(receivedMessage);
                        break;

                    case MessageType.ResolutionCalibration:
                        this.ResolutionCalibrationMethod(receivedMessage);
                        break;

                    case MessageType.ExecuteMission:
                        if (receivedMessage.Data is IExecuteMissionMessageData data)
                        {
                            await this.ExecuteMissionMethod(receivedMessage);
                            this.logger.LogDebug($"AS-AS NotificationCycle: ExecuteMission id: {data.Mission.Id}, mission quantity: {data.PendingMissionsCount}");
                        }
                        break;

                    case MessageType.BayConnected:
                        this.logger.LogDebug($"AS NotificationCycle: BayConnected received");
                        this.OnBayConnected(receivedMessage.Data as BayConnectedMessageData);
                        break;

                    // Adds other Notification Message and send it via SignalR controller
                    default:
                        break;
                }
            }
            while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("9:Method End");

            return;
        }

        #endregion
    }
}
