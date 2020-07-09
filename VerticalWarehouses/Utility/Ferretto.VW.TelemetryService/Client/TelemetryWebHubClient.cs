using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Hubs;
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
            if (this.IsConnected)
            {
                await this.SendAsync(nameof(ITelemetryHub.SendErrorLog), serialNumber, errorLog);
            }
        }

        public async Task SendMachineAsync(Machine machine)
        {
            if (this.IsConnected)
            {
                await this.SendAsync(nameof(ITelemetryHub.SendMachine), machine);
            }
        }

        public async Task SendMissionLogAsync(string serialNumber, MissionLog missionLog)
        {
            if (this.IsConnected)
            {
                await this.SendAsync(nameof(ITelemetryHub.SendMissionLog), serialNumber, missionLog);
            }
        }

        public async Task SendScreenCastAsync(int bayNumber, string serialNumber, byte[] screenshot)
        {
            if (this.IsConnected)
            {
                await this.SendAsync(nameof(ITelemetryHub.SendScreenCast), bayNumber, serialNumber, screenshot);
            }
        }

        public async Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            if (this.IsConnected)
            {
                await this.SendAsync(nameof(ITelemetryHub.SendScreenShot), bayNumber, serialNumber, timeStamp, screenshot);
            }
        }

        protected override void RegisterEvents(HubConnection connection)
        {
        }

        #endregion
    }
}
