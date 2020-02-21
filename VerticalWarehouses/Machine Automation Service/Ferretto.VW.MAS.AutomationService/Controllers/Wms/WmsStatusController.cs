using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/status")]
    [ApiController]
    public class WmsStatusController : ControllerBase
    {
        #region Fields

        private readonly IConfiguration configuration;

        #endregion

        #region Constructors

        public WmsStatusController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #endregion

        #region Methods

        [HttpGet("health")]
        public async Task<ActionResult<string>> GetHealth()
        {
            if (!this.configuration.IsWmsEnabled())
            {
                return this.UnprocessableEntity("WMS service is not enabled");
            }

            using (var client = new HttpClient() { BaseAddress = this.configuration.GetWmsServiceUrl() })
            {
                try
                {
                    var result = await client.GetAsync(new Uri(client.BaseAddress, "health/live"));
                    return this.StatusCode((int)result.StatusCode, await result.Content.ReadAsStringAsync());
                }
                catch
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }
            }
        }

        [HttpGet("enabled")]
        public ActionResult<bool> IsEnabled()
        {
            return this.Ok(this.configuration.IsWmsEnabled());
        }

        #endregion
    }
}
