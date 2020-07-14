using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Models;
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

        public async Task SendErrorLogAsync(string serialNumber, ErrorLog errorLog)
        {
            await this.SendAsync("SendErrorLog", serialNumber, errorLog);
        }

        public async Task SendMissionLogAsync(int bayNumber, string serialNumber, MissionLog missionLog)
        {
            await this.SendAsync("SendMissionLog", bayNumber, serialNumber, missionLog);
        }

        public async Task SendScreenCastAsync(int bayNumber, string serialNumber, byte[] screenshot)
        {
            await this.SendAsync("SendScreenCast", bayNumber, serialNumber, screenshot);
        }

        public async Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            await this.SendAsync("SendScreenShot", bayNumber, serialNumber, timeStamp, screenshot);
        }

        protected override void RegisterEvents(HubConnection connection)
        {
        }

        #endregion
    }
}
