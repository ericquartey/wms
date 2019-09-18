using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService
{
    internal partial class AutomationService : AutomationBackgroundService
    {

        #region Fields

        private readonly IApplicationLifetime applicationLifetime;

        private readonly IBaysDataService baysDataService;

        private readonly IBaysProvider baysProvider;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly ILogger<AutomationService> logger;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionDataService;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private List<DataModels.Bay> configuredBays;

        private IStateMachine currentStateMachine;

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
            IServiceScopeFactory serviceScopeFactory,
            IApplicationLifetime applicationLifetime,
            IBaysProvider baysProvider )
            : base(eventAggregator, logger)
        {
            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            if (applicationLifetime is null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            if (installationHub is null)
            {
                throw new ArgumentNullException(nameof(installationHub));
            }

            if (dataHubClient is null)
            {
                throw new ArgumentNullException(nameof(dataHubClient));
            }

            if (machinesDataService is null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            if (operatorHub is null)
            {
                throw new ArgumentNullException(nameof(operatorHub));
            }

            if (baysDataService is null)
            {
                throw new ArgumentNullException(nameof(baysDataService));
            }

            if (missionDataService is null)
            {
                throw new ArgumentNullException(nameof(missionDataService));
            }

            this.Logger.LogTrace("1:Method Start");

            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.machinesDataService = machinesDataService;
            this.operatorHub = operatorHub;
            this.baysDataService = baysDataService;
            this.missionDataService = missionDataService;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.baysProvider = baysProvider;
            this.applicationLifetime = applicationLifetime;
        }

        #endregion



        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.dataHubClient.ConnectAsync();
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

                switch (receivedMessage.Type)
                {
                    case MessageType.PowerEnable:
                        this.currentStateMachine = new PowerEnableStateMachine(receivedMessage, this.configuredBays, this.eventAggregator, this.logger, this.serviceScopeFactory);
                        this.currentStateMachine.Start();
                        break;

                    case MessageType.Stop:
                        this.currentStateMachine?.Stop(StopRequestReason.Stop);
                        break;
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
                    case MessageType.SensorsChanged:
                        this.OnSensorsChanged(receivedMessage);
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
                        this.CalibrateAxisMethod(receivedMessage);
                        break;

                    case MessageType.CurrentPosition:
                        this.CurrentPositionMethod(receivedMessage);
                        break;

                    case MessageType.Positioning:
                        this.OnPositioningChanged(receivedMessage);
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

                    case MessageType.ElevatorWeightCheck:
                        this.ElevatorWeightCheckMethod(receivedMessage);
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
                        this.OnInverterStatusWordChanged(receivedMessage);
                        break;

                    case MessageType.MachineStateActive:
                        this.MachineStateActiveMethod(receivedMessage);
                        break;

                    case MessageType.MachineStatusActive:
                        this.MachineStatusActiveMethod(receivedMessage);
                        break;
                }

                this.currentStateMachine?.ProcessNotificationMessage(receivedMessage);
            }
            while (!this.stoppingToken.IsCancellationRequested);

            this.logger.LogDebug("9:Method End");
        }

        private void OnDataLayerReady()
        {
            this.configuredBays = this.baysProvider.GetAll().ToList();

            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var baysConfigurationProvider = scope.ServiceProvider.GetRequiredService<IBaysConfigurationProvider>();

                baysConfigurationProvider.LoadFromConfiguration();
            }
        }

        #endregion
    }
}
