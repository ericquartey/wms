using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
                    return this.StatusCode((int)result.StatusCode, await result.Content.ReadAsStringAsync());
                }
                catch
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError, "Unhealthy");
                }
            }
        }

        [HttpGet("ip-endpoint")]
        public async Task<ActionResult<string>> GetIpEndpoint()
        {
            return this.Ok(this.wmsSettingsProvider.ServiceUrl.ToString());
        }

        [HttpGet("enabled")]
        public ActionResult<bool> IsEnabled()
        {
            return this.Ok(this.wmsSettingsProvider.IsEnabled);
        }

        [HttpPut]
        public async Task UpdateAsync(bool isEnabled, string httpUrl)
        {
            this.wmsSettingsProvider.IsEnabled = isEnabled;
            this.wmsSettingsProvider.ServiceUrl = new Uri(httpUrl);
        }

        #endregion
    }
}
