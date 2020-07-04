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

        public async Task SendScreenshot(int bayNumber, byte[] screenshot)
        {
            this.logger.LogInformation($"Screenshot size {screenshot.Length / 1024} Kb");
            await this.telemetryWebHubClient.SendScreenshotAsync(bayNumber, screenshot);
        }

        #endregion
    }
}
