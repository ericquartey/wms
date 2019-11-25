using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationProvider configurationProvider;

        #endregion

        #region Constructors

        public ConfigurationController(
            IConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<VertimagConfiguration> Get()
        {
            return this.Ok(this.configurationProvider.ConfigurationGet());
        }

        [HttpPost("import-configuration")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Import(VertimagConfiguration vertimagConfiguration, [FromServices] IServiceScopeFactory serviceScopeFactory)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));
            _ = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.configurationProvider.ConfigurationImport(vertimagConfiguration, serviceScopeFactory);
            return this.Ok();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Set(VertimagConfiguration vertimagConfiguration, [FromServices] IServiceScopeFactory serviceScopeFactory)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));
            _ = serviceScopeFactory ?? throw new System.ArgumentNullException(nameof(serviceScopeFactory));

            this.configurationProvider.ConfigurationUpdate(vertimagConfiguration, serviceScopeFactory);
            return this.Ok();
        }

        #endregion
    }
}
