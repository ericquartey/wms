using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Hubs;
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

        #region Events

        public event EventHandler MachineReceivedChanged;

        #endregion

        #region Methods

        public async Task SendErrorLogAsync(ErrorLog errorlog)
        {
            await this.SendAsync("SendErrorLog", errorlog);
        }

        public async Task SendMachineAsync(Machine machine)
        {
            await this.SendAsync("SendMachine", machine);
        }

        public async Task SendMissionLogAsync(MissionLog missionLog)
        {
            await this.SendAsync("SendMissionLog", missionLog);
        }

        public async Task SendScreenCastAsync(int bayNumber, byte[] screenshot)
        {
            await this.SendAsync("SendScreenCast", bayNumber, screenshot);
        }

        public async Task SendScreenShotAsync(int bayNumber, DateTimeOffset timeSpan, byte[] screenShot)
        {
            await this.SendAsync("SendScreenShot", bayNumber, timeSpan, screenShot);
        }

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On(nameof(ITelemetryHub.RequestMachine), this.OnRequestMachine);
        }

        private void OnRequestMachine()
        {
            this.MachineReceivedChanged?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
