using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationProvider configurationProvider;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public ConfigurationController(IConfigurationProvider configurationProvider,
            IMachineProvider machineProvider)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<VertimagConfiguration> Get()
        {
            return this.Ok(this.configurationProvider.ConfigurationGet());
        }

        [HttpPost("get/config")]
        public ActionResult<Machine> GetConfig()
        {
            return this.Ok(this.machineProvider.GetMinMaxHeight());
        }

        [HttpPost("get/machine")]
        public ActionResult<Machine> GetMachine()
        {
            return this.Ok(this.machineProvider.Get());
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

        [HttpPost("update-parameter")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Update(Machine machineNew)
        {
            _ = machineNew ?? throw new ArgumentNullException(nameof(machineNew));

            this.configurationProvider.UpdateMachine(machineNew);
            return this.Ok();
        }

        [HttpPost("get-json-configuration")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<VertimagConfiguration> GetJsonConfiguration()
        {
            return this.Ok(this.configurationProvider.GetJsonConfiguration());
        }

        #endregion
    }
}
