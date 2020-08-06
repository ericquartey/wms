using System;
using System.Net;
using System.Net.Http;
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

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public WmsStatusController(IWmsSettingsProvider wmsSettingsProvider)
        {
            this.wmsSettingsProvider = wmsSettingsProvider;
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

            using (var client = new HttpClient() { BaseAddress = this.wmsSettingsProvider.ServiceUrl })
            {
                try
                {
                    var result = await client.GetAsync(new Uri(client.BaseAddress, "health/live"));
                    var statusString = await result.Content.ReadAsStringAsync();
                    if (Enum.TryParse<HealthStatus>(statusString, out var status))
                    {
                        this.wmsSettingsProvider.IsConnected = (status == HealthStatus.Healthy);
                    }
                    return this.StatusCode((int)result.StatusCode, statusString);
                }
                catch
                {
                    this.wmsSettingsProvider.IsConnected = false;
                    return this.StatusCode((int)HttpStatusCode.InternalServerError, "Unhealthy");
                }
            }
        }

        [HttpGet("ip-endpoint")]
        public async Task<ActionResult<string>> GetIpEndpoint()
        {
            return this.Ok(this.wmsSettingsProvider.ServiceUrl?.ToString());
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

        [HttpPut]
        public async Task UpdateAsync(bool isEnabled, string httpUrl)
        {
            if (isEnabled && string.IsNullOrEmpty(httpUrl))
            {
                throw new ArgumentException("The url must be specified");
            }

            this.wmsSettingsProvider.IsEnabled = isEnabled;
            this.wmsSettingsProvider.ServiceUrl = httpUrl is null ? null : new Uri(httpUrl);
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

        #endregion
    }
}
