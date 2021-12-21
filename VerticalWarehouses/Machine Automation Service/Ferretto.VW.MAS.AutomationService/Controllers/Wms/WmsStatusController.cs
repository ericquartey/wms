using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/status")]
    [ApiController]
    public class WmsStatusController : ControllerBase
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public WmsStatusController(
            IWmsSettingsProvider wmsSettingsProvider,
            IErrorsProvider errorsProvider)
        {
            this.wmsSettingsProvider = wmsSettingsProvider;
            this.errorsProvider = errorsProvider;
        }

        #endregion

        #region Methods

        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetHealth()
        {
            if (!this.wmsSettingsProvider.IsEnabled)
            {
                return this.UnprocessableEntity("WMS service is not enabled");
            }

            int numCycle = 25;
            return await this.CheckWmsHealth(numCycle);
        }

        [HttpGet("ip-endpoint")]
        public async Task<ActionResult<string>> GetIpEndpoint()
        {
            return this.Ok(this.wmsSettingsProvider.ServiceUrl?.ToString());
        }

        [HttpGet("get-socketlink-polling")]
        public ActionResult<int> GetSocketLinkPolling()
        {
            return this.Ok(this.wmsSettingsProvider.SocketLinkPolling);
        }

        [HttpGet("get-socketlink-port")]
        public ActionResult<int> GetSocketLinkPort()
        {
            return this.Ok(this.wmsSettingsProvider.SocketLinkPort);
        }

        [HttpGet("get-socketlink-timeout")]
        public ActionResult<int> GetSocketLinkTimeout()
        {
            return this.Ok(this.wmsSettingsProvider.SocketLinkTimeout);
        }

        [HttpGet("get-time-sync-interval-milliseconds")]
        public ActionResult<int> GetTimeSyncIntervalMilliseconds()
        {
            return this.Ok(this.wmsSettingsProvider.TimeSyncIntervalMilliseconds);
        }

        [HttpGet("enabled")]
        public ActionResult<bool> IsEnabled()
        {
            return this.Ok(this.wmsSettingsProvider.IsEnabled);
        }

        [HttpGet("is-time-sync-enabled")]
        public ActionResult<bool> IsTimeSyncEnabled()
        {
            return this.Ok(this.wmsSettingsProvider.IsTimeSyncEnabled);
        }

        [HttpGet("socketlink-is-enabled")]
        public ActionResult<bool> SocketLinkIsEnabled()
        {
            return this.Ok(this.wmsSettingsProvider.SocketLinkIsEnabled);
        }

        [HttpPut]
        public async Task UpdateAsync(bool isEnabled, string httpUrl, bool socketLinkIsEnabled, int socketLinkPort, int socketLinkTimeout, int socketLinkPolling)
        {
            if (isEnabled && string.IsNullOrEmpty(httpUrl))
            {
                throw new ArgumentException("The url must be specified");
            }

            if (isEnabled && socketLinkIsEnabled)
            {
                throw new ArgumentException("The Wms and Socket Link connot be enabled at the same time");
            }

            this.wmsSettingsProvider.IsEnabled = isEnabled;
            this.wmsSettingsProvider.ServiceUrl = httpUrl is null ? null : new Uri(httpUrl);

            this.wmsSettingsProvider.SocketLinkIsEnabled = socketLinkIsEnabled;
            this.wmsSettingsProvider.SocketLinkPort = socketLinkPort;
            this.wmsSettingsProvider.SocketLinkTimeout = socketLinkTimeout;
            this.wmsSettingsProvider.SocketLinkPolling = socketLinkPolling;
        }

        [HttpPut("update-wms-time-settings")]
        public async Task UpdateIsTimeSyncEnabledAsync()
        {
            this.wmsSettingsProvider.IsTimeSyncEnabled = true;
        }

        [HttpPost("time-sync-interval-milliseconds-update")]
        public async Task UpdateTimeSyncIntervalMilliseconds(int seconds)
        {
            if (seconds <= 0)
            {
                throw new ArgumentException("The time interval must be positive");
            }

            this.wmsSettingsProvider.TimeSyncIntervalMillisecondsUpdate(seconds);
        }

        private async Task<ActionResult<string>> CheckWmsHealth(int numCycle)
        {
            var startTime = DateTime.Now;
            var statusCode = this.StatusCode((int)HealthStatus.Unhealthy, "Unhealthy");
            //var timeout = this.wmsSettingsProvider.SocketLinkTimeout > 0 ? this.wmsSettingsProvider.SocketLinkTimeout : 5000;
            var timeout = 5000;
            for (int i = 1; i <= numCycle && (DateTime.Now - startTime).TotalMilliseconds < timeout; i++)
            {
                using (var client = new HttpClient() { BaseAddress = this.wmsSettingsProvider.ServiceUrl })
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        var result = await client.GetAsync(new Uri(client.BaseAddress, "health/live"));
                        var statusString = await result.Content.ReadAsStringAsync();
                        if (Enum.TryParse<HealthStatus>(statusString, out var status))
                        {
                            this.wmsSettingsProvider.IsConnected = (status == HealthStatus.Healthy);
                        }

                        if (this.wmsSettingsProvider.IsConnected)
                        {
                            statusCode = this.StatusCode((int)result.StatusCode, statusString);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.wmsSettingsProvider.IsConnected = false;
                        statusCode = this.StatusCode((int)HealthStatus.Unhealthy, $"{i}: {ex.Message}");
                        Thread.Sleep(1000);
                    }
                }
            }
            if (statusCode.StatusCode == (int)HealthStatus.Unhealthy)
            {
                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, additionalText: statusCode.Value.ToString());
            }
            return statusCode;
        }

        #endregion
    }
}
