using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Hubs;
using Ferretto.ServiceDesk.Telemetry.Models;
using Ferretto.VW.TelemetryService.Provider;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService
{
    public class TelemetryHub : Hub<ITelemetryHub>
    {
        #region Fields

        private readonly ILogger<TelemetryHub> logger;

        private readonly IMachineProvider machineProvider;

        private readonly ITelemetryWebHubClient telemetryWebHubClient;

        #endregion

        #region Constructors

        public TelemetryHub(ITelemetryWebHubClient telemetryWebHubClient,
                            IMachineProvider machineProvider,
                            ILogger<TelemetryHub> logger)
        {
            this.telemetryWebHubClient = telemetryWebHubClient;
            this.machineProvider = machineProvider;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var machine = await this.machineProvider.GetAsync();
            if (machine is null)
            {
                await this.Clients.Caller.RequestMachine();
            }
        }

        public async Task SendErrorLog(ErrorLog errorLog)
        {
            this.logger.LogInformation($"ErrorLog");
            var machine = await this.machineProvider.GetAsync();
            await this.telemetryWebHubClient.SendErrorLogAsync(machine?.SerialNumber, errorLog);
        }

        public async Task SendMachine(Machine machine)
        {
            this.logger.LogInformation($"machine");
            var newMachine = new Models.Machine
            {
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
            };

            await this.machineProvider.SaveAsync(newMachine);
            await this.telemetryWebHubClient.SendMachineAsync(machine);
        }

        public async Task SendMissionLog(MissionLog missionLog)
        {
            this.logger.LogInformation($"MissionLog");
            var machine = await this.machineProvider.GetAsync();
            await this.telemetryWebHubClient.SendMissionLogAsync(machine?.SerialNumber, missionLog);
        }

        public async Task SendScreenCast(int bayNumber, byte[] screenshot)
        {
            this.logger.LogInformation($"Screencast image size {screenshot.Length / 1024} Kb");
            var machine = await this.machineProvider.GetAsync();
            await this.telemetryWebHubClient.SendScreenCastAsync(bayNumber, machine?.SerialNumber, screenshot);
        }

        public async Task SendScreenShot(int bayNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            this.logger.LogInformation($"Screenshot image size {screenshot.Length / 1024} Kb");
            var machine = await this.machineProvider.GetAsync();
            await this.telemetryWebHubClient.SendScreenShotAsync(bayNumber, machine?.SerialNumber, timeStamp, screenshot);
        }

        #endregion
    }
}
