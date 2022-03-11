using System;
using System.Net;
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

        public event EventHandler<ProxyChangedEventArgs> ProxyReceivedChanged;

        #endregion

        #region Methods

        public async Task GetProxyAsync()
        {
            if (!this.IsConnected)
            {
                this.logger.Debug($"Error sending get proxy to telemetry service: not connected");
                return;
            }

            try
            {
                await this.SendAsync("GetProxy");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending get proxy to telemetry service {ex.Message}");
            }
        }

        public async Task SendErrorLogAsync(IErrorLog errorlog)
        {
            if (!this.IsConnected)
            {
                this.logger.Debug($"Error sending error log to telemetry service: not connected");
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
                //await this.SendAsync("SendIOLog", ioLog);
                await this.SendAsync("PersistIOLog", ioLog);
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

        public async Task SendProxyAsync(IProxy proxy)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendProxy", proxy);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending proxy to telemetry service {ex.Message}");
            }
        }

        public async Task SendRawDatabaseContentAsync(byte[] rawDatabaseContent)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                //x await this.SendRawDatabaseContentAsync(rawDatabaseContent);
                await this.SendAsync("SendRawDatabaseContent", rawDatabaseContent);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending raw database content to telemetry service {ex.Message}");
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

        public async Task SendServicingInfoAsync(IServicingInfo servicingInfo)
        {
            if (!this.IsConnected)
            {
                return;
            }

            try
            {
                await this.SendAsync("SendServicingInfo", servicingInfo);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error sending service info to telemetry service {ex.Message}");
            }
        }

        protected override void RegisterEvents(HubConnection connection)
        {
            connection.On(nameof(ITelemetryHub.RequestMachine), this.OnRequestMachine);
            connection.On<WebProxy>(nameof(ITelemetryHub.GetProxy), this.OnGetProxy);
        }

        private void OnGetProxy(WebProxy proxy)
        {
            this.ProxyReceivedChanged?.Invoke(this, new ProxyChangedEventArgs(proxy));
        }

        private void OnRequestMachine()
        {
            this.MachineReceivedChanged?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
