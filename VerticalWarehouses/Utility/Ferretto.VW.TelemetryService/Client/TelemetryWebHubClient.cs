using System;
using System.Threading.Tasks;
using Ferretto.VW.Common.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.TelemetryService
{
    public class TelemetryWebHubClient : AutoReconnectHubClient, ITelemetryWebHubClient
    {
        #region Constructors

        public TelemetryWebHubClient(Uri uri)
            : base(uri)
        {
        }

        #endregion

        #region Methods

        public async Task SendLogsAsync(int bayNumber)
        {
            await this.SendAsync("SendLogs", bayNumber);
        }

        public async Task SendScreenshotAsync(int bayNumber, byte[] screenshot)
        {
            await this.SendAsync("SendScreenshot", bayNumber, screenshot);
        }

        protected override void RegisterEvents(HubConnection connection)
        {
        }

        #endregion
    }
}
