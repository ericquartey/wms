using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ferretto.VW.MachineAutomationService.Contracts;
using Ferretto.VW.MachineAutomationService.Hubs;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core
{
    public class MachineLiveDataService : BackgroundService
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IHubContext<DataHub, IDataHub> dataHubContext;

        private readonly IMachinesLiveDataContext liveMachinesDataContext;

        private readonly ILogger<MachineLiveDataService> logger;

        private readonly IMapper mapper;

        private readonly IServiceScopeFactory scopeFactory;

        #endregion

        #region Constructors

        public MachineLiveDataService(
            ILogger<MachineLiveDataService> logger,
            IConfiguration configuration,
            IMapper mapper,
            IServiceScopeFactory scopeFactory,
            IMachinesLiveDataContext liveMachinesDataContext,
            IHubContext<DataHub, IDataHub> dataHubContext)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.configuration = configuration;
            this.scopeFactory = scopeFactory;
            this.liveMachinesDataContext = liveMachinesDataContext;
            this.dataHubContext = dataHubContext;
        }

        #endregion

        #region Methods

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var machineHub in this.liveMachinesDataContext.MachineHubs)
            {
                await machineHub.DisconnectAsync();
            }

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = this.scopeFactory.CreateScope())
            {
                var machineProvider = scope.ServiceProvider.GetService<IMachineProvider>();
                var bayProvider = scope.ServiceProvider.GetService<IBayProvider>();

                var result = await machineProvider.GetAllMachinesServiceInfoAsync();
                if (!result.Success)
                {
                    this.logger.LogError(
                       $"Unable to retrieve machines for live data initialization.");
                }

                var machines = result.Entity;
                var bays = await bayProvider.GetAllAsync();

                var machineHubPath = this.configuration.GetMachineHubPath();
                foreach (var machine in machines)
                {
                    var machineStatus = this.liveMachinesDataContext.GetMachineStatus(machine.Id);

                    machineStatus.BaysStatus = bays
                        .Where(b => b.MachineId == machine.Id)
                        .Select(b => new BayStatus
                        {
                            BayId = b.Id,
                        }).ToArray();

                    if (System.Uri.TryCreate(machine.ServiceUrl, System.UriKind.Absolute, out var machineServiceUrl))
                    {
                        var machineHubUri = new System.Uri(machineServiceUrl, machineHubPath);

                        this.liveMachinesDataContext.MachineHubs.Add(
                            new MachineHubClient(machineHubUri, machine.Id));
                    }
                    else
                    {
                        this.logger.LogError(
                            $"Service Url for machine id={machine.Id} is invalid. No connection will be established to the machine's live data hub.");
                    }
                }

                await Task.WhenAll(this.liveMachinesDataContext.MachineHubs.Select(this.ConfigureHubAsync));
            }
        }

        private async Task ConfigureHubAsync(IMachineHubClient machineHubClient)
        {
            machineHubClient.ConnectionStatusChanged += this.MachineHubClient_ConnectionStatusChanged;
            machineHubClient.ModeChanged += this.MachineHubClient_ModeChanged;
            machineHubClient.MachineStatusReceived += this.MachineHubClient_MachineStatusReceived;
            machineHubClient.ElevatorPositionChanged += this.MachineHubClient_ElevatorPositionChanged;
            machineHubClient.UserChanged += this.MachineHubClient_UserChanged;
            machineHubClient.LoadingUnitInBayChanged += this.MachineHubClient_LoadingUnitInBayChanged;
            machineHubClient.MaxReconnectTimeoutMilliseconds = this.configuration.GetMaxMachineReconnectTimeoutMilliseconds();

            this.logger.LogInformation($"Connecting to live machine hub (machine id={machineHubClient.MachineId}) ...");
            await machineHubClient.ConnectAsync();
        }

        private async void MachineHubClient_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (!e.IsConnected)
            {
                this.logger.LogWarning($"Disconnected from machine (id={e.MachineId})");

                var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

                machineStatus.Mode = Enums.MachineStatus.Offline;

                this.NotifyUpdate(e.MachineId);
            }
            else
            {
                this.logger.LogInformation($"Connected to live machine hub (machine id={e.MachineId})");

                var machineHub = this.liveMachinesDataContext.MachineHubs
                    .SingleOrDefault(hub => hub.MachineId == e.MachineId);

                await machineHub.RequestCurrentStateAsync();
            }
        }

        private void MachineHubClient_ElevatorPositionChanged(object sender, ElevatorPositionChangedEventArgs e)
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

            if (machineStatus.ElevatorStatus == null)
            {
                machineStatus.ElevatorStatus = new ElevatorStatus();
            }

            machineStatus.ElevatorStatus.Position = e.Position;

            this.NotifyUpdate(e.MachineId);
        }

        private void MachineHubClient_LoadingUnitInBayChanged(object sender, LoadingUnitChangedEventArgs e)
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

            var bayStatus = machineStatus.BaysStatus.SingleOrDefault(b => b.BayId == e.BayId);
            if (bayStatus != null)
            {
                bayStatus.LoadingUnitId = e.LoadingUnitId;

                this.NotifyUpdate(e.MachineId);
            }
        }

        private void MachineHubClient_MachineStatusReceived(object sender, MachineStatusReceivedEventArgs e)
        {
            if (e.MachineStatus == null)
            {
                return;
            }

            var newMachineStatus = e.MachineStatus;

            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineStatus.MachineId);
            machineStatus.FaultCode = newMachineStatus.FaultCode;
            machineStatus.Mode = newMachineStatus.Mode;
            machineStatus.ElevatorStatus = newMachineStatus.ElevatorStatus;

            foreach (var bayStatus in machineStatus.BaysStatus)
            {
                var newBayStatus = machineStatus.BaysStatus.SingleOrDefault(b => b.BayId == bayStatus.BayId);
                if (newBayStatus != null)
                {
                    bayStatus.LoadingUnitId = newBayStatus.LoadingUnitId;
                    bayStatus.LoggedUserId = newBayStatus.LoggedUserId;
                }
            }

            this.NotifyUpdate(e.MachineStatus.MachineId);
        }

        private async void MachineHubClient_ModeChanged(object sender, ModeChangedEventArgs e)
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

            if (machineStatus.Mode == Enums.MachineStatus.Offline)
            {
                var machineHub = this.liveMachinesDataContext.MachineHubs
                    .SingleOrDefault(hub => hub.MachineId == e.MachineId);

                if (machineHub != null)
                {
                    await machineHub.RequestCurrentStateAsync();
                }
            }
            else
            {
                machineStatus.Mode = e.Mode;
                if (e.Mode == Enums.MachineStatus.Error)
                {
                    machineStatus.FaultCode = e.FaultCode;
                }
                else
                {
                    machineStatus.FaultCode = null;
                }
            }

            this.NotifyUpdate(e.MachineId);
        }

        private void MachineHubClient_UserChanged(object sender, UserChangedEventArgs e)
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

            var bayStatus = machineStatus.BaysStatus.SingleOrDefault(b => b.BayId == e.BayId);
            if (bayStatus != null)
            {
                bayStatus.LoggedUserId = e.UserId;

                this.NotifyUpdate(e.MachineId);
            }
        }

        private void NotifyUpdate(int machineId)
        {
            if (this.dataHubContext.Clients != null)
            {
                this.dataHubContext.Clients.All.MachineStatusUpdated(
                    this.mapper.Map<Data.Hubs.Models.MachineStatus>(this.liveMachinesDataContext.GetMachineStatus(machineId)));
            }
        }

        #endregion
    }
}
