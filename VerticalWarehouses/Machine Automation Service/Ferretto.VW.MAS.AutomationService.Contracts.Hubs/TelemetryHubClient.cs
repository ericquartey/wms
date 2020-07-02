using System;
using Ferretto.VW.MAS.AutomationService.Hubs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class TelemetryHubClient : AutoReconnectHubClient, ITelemetryHubClient
    {
        #region Constructors

        public TelemetryHubClient(Uri uri)
            : base(uri)
        {
        }

        #endregion

        #region Methods

        public async Task SendLogsAsync(BayNumber bayNumber)
        {
            await this.SendAsync(nameof(ITelemetryHub.SendLogs), bayNumber);
        }

        public async Task SendScreenshotAsync(BayNumber bayNumber, byte[] image)
        {
            await this.SendAsync(nameof(ITelemetryHub.SendScreenShot), bayNumber, image);
        }

        protected override void RegisterEvents(HubConnection connection)
        {
        }

        #endregion
    }
}
