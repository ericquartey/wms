using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MachineAutomationService.Contracts;
using Ferretto.VW.MachineAutomationService.Hubs;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core
{
    public class MachineLiveDataService : BackgroundService
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly ILiveMachinesDataContext liveMachinesDataContext;

        private readonly ILogger<MachineLiveDataService> logger;

        private readonly IHubContext<DataHub, IDataHub> schedulerHubContext;

        private readonly IServiceScopeFactory scopeFactory;

        #endregion

        #region Constructors

        public MachineLiveDataService(
            ILogger<MachineLiveDataService> logger,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILiveMachinesDataContext liveMachinesDataContext,
            IHubContext<DataHub, IDataHub> schedulerHubContext)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.scopeFactory = scopeFactory;
            this.liveMachinesDataContext = liveMachinesDataContext;
            this.schedulerHubContext = schedulerHubContext;
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
            IEnumerable<Models.Machine> machines = null;
            var machineHubPath = this.configuration["Hubs:Machine"];

            using (var scope = this.scopeFactory.CreateScope())
            {
                var machineProvider = scope.ServiceProvider.GetService<IMachineProvider>();
                var bayProvider = scope.ServiceProvider.GetService<IBayProvider>();

                machines = await machineProvider.GetAllAsync(0, int.MaxValue);
                var bays = await bayProvider.GetAllAsync();

                foreach (var machine in machines)
                {
                    var machineStatus = this.liveMachinesDataContext.GetMachineStatus(machine.Id);

                    machineStatus.BaysStatus = bays
                        .Where(b => b.MachineId == machine.Id)
                        .Select(b => new BayStatus
                        {
                            BayId = b.Id
                        });

                    this.liveMachinesDataContext.MachineHubs.Add(
                        new MachineHubClient(new System.Uri($"http://localhost:9000/{machineHubPath}"), machine.Id));
                }
            }

            foreach (var machineHub in this.liveMachinesDataContext.MachineHubs)
            {
                await this.ConfigureHubAsync(machineHub);
            }
        }

#pragma warning restore S125 // Sections of code should not be commented out

        private async Task ConfigureHubAsync(IMachineHubClient machineHubClient)
        {
            machineHubClient.ConnectionStatusChanged += this.MachineHubClient_ConnectionStatusChanged;
            machineHubClient.ModeChanged += this.MachineHubClient_ModeChanged;
            machineHubClient.MachineStatusReceived += this.MachineHubClient_MachineStatusReceived;

            this.logger.LogInformation($"Connecting to machine hub ...");
            await machineHubClient.ConnectAsync();
        }

        private async void MachineHubClient_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected == false)
            {
                this.logger.LogWarning($"Disconnected from machine (id={e.MachineId})");

                var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

                machineStatus.Mode = MachineMode.Offline;

                this.NotifyUpdate(e.MachineId);
            }
            else
            {
                this.logger.LogInformation($"Connected to machine (id={e.MachineId})");

                var machineHub = this.liveMachinesDataContext.MachineHubs
                    .SingleOrDefault(hub => hub.MachineId == e.MachineId);

                await machineHub.RequestCurrentStateAsync();
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
            machineStatus.Mode = newMachineStatus.Mode != MachineMode.Offline ? newMachineStatus.Mode : machineStatus.Mode;
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
        }

        private async void MachineHubClient_ModeChanged(object sender, ModeChangedEventArgs e)
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(e.MachineId);

            if (machineStatus.Mode == MachineMode.Offline)
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
                if (e.Mode == MachineMode.Fault)
                {
                    machineStatus.FaultCode = e.FaultCode;
                }
            }

            this.NotifyUpdate(e.MachineId);
        }

        private void NotifyUpdate(int machineId)
        {
            this.schedulerHubContext.Clients.All.EntityUpdated(
                new EntityChangedHubEvent
                {
                    Id = machineId,
                    EntityType = "Machine",
                    Operation = HubEntityOperation.Updated
                });
        }

        #endregion
    }
}
