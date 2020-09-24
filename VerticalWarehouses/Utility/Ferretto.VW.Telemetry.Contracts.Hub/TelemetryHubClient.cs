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
        #region Fields

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

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
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendErrorLog", errorlog);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending error log to telemetry service {ex.Message}");
            }
        }

        public async Task SendIOLogAsync(IIOLog ioLog)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendIOLog", ioLog);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending IO log to telemetry service {ex.Message}");
            }
        }

        public async Task SendMachineAsync(IMachine machine)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendMachine", machine);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending machine to telemetry service {ex.Message}");
            }
        }

        public async Task SendMissionLogAsync(IMissionLog missionLog)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendMissionLog", missionLog);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending mission log to telemetry service {ex.Message}");
            }
        }

        public async Task SendScreenCastAsync(int bayNumber, byte[] screenshot, DateTimeOffset timeStamp)
        {
            if (!this.IsConnected)
            {
                return;
            }
            try
            {
                await this.SendAsync("SendScreenCast", bayNumber, timeStamp, screenshot);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending SendScreenCast to telemetry service {ex.Message}");
            }
        }

        public async Task SendScreenShotAsync(int bayNumber, DateTimeOffset timeStamp, byte[] screenShot)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendScreenShot", bayNumber, timeStamp, screenShot);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending screenshot to telemetry service {ex.Message}");
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
