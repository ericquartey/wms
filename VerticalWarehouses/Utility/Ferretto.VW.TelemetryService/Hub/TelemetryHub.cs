using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService
{
    public class TelemetryHub : Hub<ITelemetryHub>
    {
        #region Fields

        private readonly ILogger<TelemetryHub> logger;

        private readonly ITelemetryWebHubClient telemetryWebHubClient;

        #endregion

        #region Constructors

        public TelemetryHub(ITelemetryWebHubClient telemetryWebHubClient,
                            ILogger<TelemetryHub> logger)
        {
            this.telemetryWebHubClient = telemetryWebHubClient;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public Task SendLogs(int bayNumber)
        {
            //
            return null;
        }

        public async Task SendScreenCast(int bayNumber, string serialNumber, byte[] screenshot)
        {
            this.logger.LogInformation($"Screencast image size {screenshot.Length / 1024} Kb");
            await this.telemetryWebHubClient.SendScreenCastAsync(bayNumber, serialNumber, screenshot);
        }

        public async Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            this.logger.LogInformation($"Screenshot image size {screenshot.Length / 1024} Kb");
            await this.telemetryWebHubClient.SendScreenShotAsync(bayNumber, serialNumber, timeStamp, screenshot);
        }

        #endregion
    }
}
