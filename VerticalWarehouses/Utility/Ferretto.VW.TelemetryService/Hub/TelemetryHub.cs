using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Hubs;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService
{
    public class TelemetryHub : Hub<ITelemetryHub>
    {
        #region Fields

        private readonly ILogger<TelemetryHub> logger;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly ITelemetryWebHubClient telemetryWebHubClient;

        #endregion

        #region Constructors

        public TelemetryHub(
            ITelemetryWebHubClient telemetryWebHubClient,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TelemetryHub> logger)
        {
            this.telemetryWebHubClient = telemetryWebHubClient;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogDebug("No machine is defined in the database. Requesting identification to connected client.");
                await this.Clients.Caller.RequestMachine();
            }
        }

        public async Task PersistIOLog(IOLog ioLog)
        {
            if (ioLog is null)
            {
                return;
            }

            this.logger.LogDebug($"Received IO log from client.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to persist a IO log with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.PersistIOLogAsync(machine.SerialNumber, ioLog);
        }

        public async Task SendErrorLog(ErrorLog errorLog)
        {
            if (errorLog is null)
            {
                return;
            }

            this.logger.LogDebug($"Received error log from client.");
            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send an error log with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendErrorLogAsync(machine.SerialNumber, errorLog);
        }

        public async Task SendIOLog(IOLog ioLog)
        {
            if (ioLog is null)
            {
                return;
            }

            this.logger.LogDebug($"Received IO log from client.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send a IO log with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendIOLogAsync(machine.SerialNumber, ioLog);
        }

        public async Task SendMachine(Machine machine)
        {
            if (machine is null)
            {
                return;
            }

            this.logger.LogDebug(
                "Received machine identification from client. Machine is '{model}', '{serial}'.",
                machine.ModelName,
                machine.SerialNumber);

            using var scope = this.serviceScopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IMachineProvider>().SaveAsync(machine);

            await this.telemetryWebHubClient.SendMachineAsync(machine);
        }

        public async Task SendMissionLog(MissionLog missionLog)
        {
            if (missionLog is null)
            {
                return;
            }

            this.logger.LogDebug($"Received mission log from client.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send a mission log with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendMissionLogAsync(machine.SerialNumber, missionLog);
        }

        public async Task SendScreenCast(int bayNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            this.logger.LogDebug($"Received screencast image (size {screenshot.Length / 1024} Kb) from client.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to screen cast with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendScreenCastAsync(bayNumber, machine.SerialNumber, timeStamp, screenshot);
        }

        public async Task SendScreenShot(int bayNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            this.logger.LogInformation($"Screenshot image size {screenshot.Length / 1024} Kb");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send a screenshot with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendScreenShotAsync(bayNumber, machine.SerialNumber, timeStamp, screenshot);
        }

        #endregion
    }
}
