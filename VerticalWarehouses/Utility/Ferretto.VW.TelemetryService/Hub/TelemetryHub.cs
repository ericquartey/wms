using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.ServiceDesk.Telemetry.Hubs;
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

        public async Task GetProxy()
        {
            using var scope = this.serviceScopeFactory.CreateScope();
            try
            {
                var proxy = scope.ServiceProvider.GetRequiredService<IProxyProvider>().Get();
                if (proxy != null)
                {
                    await this.Clients.Caller.RequestProxy(proxy);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            using var scope = this.serviceScopeFactory.CreateScope();
            try
            {
                //var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
                //if (machine is null)
                //{
                this.logger.LogDebug("Requesting identification to connected client.");
                await this.Clients.Caller.RequestMachine();
                //}
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
            }
        }

        public async Task PersistIOLog(IOLog ioLog)
        {
            if (ioLog is null)
            {
                return;
            }

            this.logger.LogDebug($"Save IO log from client.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to persist a IO log with no machine defined in the local database.");
                return;
            }

            this.telemetryWebHubClient.PersistIOLogAsync(machine.SerialNumber, ioLog);
        }

        public async Task SendErrorLog(ErrorLog errorLog)
        {
            if (errorLog is null)
            {
                this.logger.LogWarning("Trying to send an error log with null parameter.");
                return;
            }

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send an error log with no machine defined in the local database.");
                return;
            }
            this.logger.LogDebug($"Received error log from client. serialNumber {machine.SerialNumber}. code {errorLog.Code}. detailCode {errorLog.DetailCode}. errorId {errorLog.ErrorId}");

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
                "Received machine identification from client. Machine is '{model}', '{serial}', wms version '{}'.",
                machine.ModelName,
                machine.SerialNumber,
                machine.WmsVersion);

            using var scope = this.serviceScopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMachineProvider>().SaveAsync(machine);

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

        public async Task SendProxy(Proxy proxy)
        {
            this.logger.LogInformation($"Received proxy identification from client. Url is '{proxy?.Url}', user '{proxy?.User}'.");

            using var scope = this.serviceScopeFactory.CreateScope();
            if (proxy is null || proxy.Url is null)
            {
                scope.ServiceProvider.GetRequiredService<IProxyProvider>().SaveAsync(null);
                await this.telemetryWebHubClient.SetProxy(null);
                return;
            }

            try
            {
                scope.ServiceProvider.GetRequiredService<IProxyProvider>().SaveAsync(proxy);
                var webProxy = scope.ServiceProvider.GetRequiredService<IProxyProvider>().GetWebProxy();

                await this.telemetryWebHubClient.SetProxy(webProxy);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message, ex);
            }
        }

        public async Task SendRawDatabaseContent(byte[] rawDatabaseContent)
        {
            if (rawDatabaseContent is null)
            {
                return;
            }

            this.logger.LogDebug($"Received raw database content from client, size {rawDatabaseContent.Length / 1024} Kb.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send a raw database content with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendRawDatabaseContentAsync(machine.SerialNumber, rawDatabaseContent);
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

        public async Task SendServicingInfo(ServicingInfo servicingInfo)
        {
            if (servicingInfo is null)
            {
                return;
            }

            this.logger.LogDebug($"Received servicing info from client.");

            using var scope = this.serviceScopeFactory.CreateScope();
            var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
            if (machine is null)
            {
                this.logger.LogWarning("Trying to send a servicing info with no machine defined in the local database.");
                return;
            }

            await this.telemetryWebHubClient.SendServicingInfoAsync(machine.SerialNumber, servicingInfo);
        }

        #endregion
    }
}
