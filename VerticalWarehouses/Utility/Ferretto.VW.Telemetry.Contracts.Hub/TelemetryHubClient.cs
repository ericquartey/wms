using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Hubs;
using Ferretto.ServiceDesk.Telemetry;
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

        public async Task SendErrorLogAsync(IErrorLog errorlog)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendErrorLog", errorlog);
            }
        }

        public async Task SendMachineAsync(IMachine machine)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendMachine", machine);
            }
        }

        public async Task SendMissionLogAsync(IMissionLog missionLog)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendMissionLog", missionLog);
            }
        }

        public async Task SendScreenCastAsync(int bayNumber, byte[] screenshot, DateTimeOffset timeStamp)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendScreenCast", bayNumber, timeStamp, screenshot);
            }
        }

        public async Task SendScreenShotAsync(int bayNumber, DateTimeOffset timeStamp, byte[] screenShot)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendScreenShot", bayNumber, timeStamp, screenShot);
            }
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
