using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Models;
using Ferretto.VW.Common.Hubs;
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

        public async Task SendErrorLog(string serialNumber, ErrorLog errorlog)
        {
            await this.SendAsync("SendErrorLog", serialNumber, errorlog);
        }

        public async Task SendMissionLog(string serialNumber, MissionLog missionLog)
        {
            await this.SendAsync("SendMissionLog", serialNumber, missionLog);
        }

        public async Task SendScreenCastAsync(int bayNumber, string serialNumber, byte[] screenshot)
        {
            await this.SendAsync("SendScreenCast", bayNumber, serialNumber, screenshot);
        }

        public async Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeSpan, byte[] screenShot)
        {
            await this.SendAsync("SendScreenShot", bayNumber, serialNumber, timeSpan, screenShot);
        }

        protected override void RegisterEvents(HubConnection connection)
        {
        }

        #endregion
    }
}
