using System;
using System.Threading.Tasks;
using Ferretto.VW.Common.Hubs;
using Ferretto.VW.Telemetry.Contracts.Hub.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace Ferretto.VW.Telemetry.Contracts.Hub
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

        public async Task SendLogsAsync(int bayNumber)
        {
            await this.SendAsync("SendLogs", bayNumber);
        }

        public async Task SendMissionLog(string serialNumber, MissionLog missionLog)
        {
            await this.SendAsync("SendMissionLog", serialNumber, missionLog);
        }

        public async Task SendScreenshotAsync(int bayNumber, byte[] screenshot)
        {
            await this.SendAsync("SendScreenshot", bayNumber, screenshot);
        }

        //public async Task SendScreenshotAsync(int bayNumber, Screenshot screenshot)
        //{
        //    await this.SendAsync("SendScreenshot", bayNumber, screenshot);
        //}

        protected override void RegisterEvents(HubConnection connection)
        {
        }

        #endregion
    }
}
