using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ConfigurationController(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsProvider loadingUnitsProvider,
            IMachineProvider machineProvider)
        {
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitsProvider));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new System.ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<VertimagConfiguration> Get()
        {
            var configuration = new VertimagConfiguration
            {
                Machine = this.machineProvider.Get(),
                SetupProcedures = this.setupProceduresDataProvider.GetAll(),
                LoadingUnits = this.loadingUnitsProvider.GetAll(),
            };

            return this.Ok(configuration);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Set(VertimagConfiguration vertimagConfiguration, [FromServices] IServiceScopeFactory serviceScopeFactory)
        {
            if (vertimagConfiguration is null)
            {
                throw new System.ArgumentNullException(nameof(vertimagConfiguration));
            }

            // 3 scopes, avoid cross reference of same object on save

            using (var scope = serviceScopeFactory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IMachineProvider>().Update(vertimagConfiguration.Machine);
            }

            using (var scope = serviceScopeFactory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>().Update(vertimagConfiguration.SetupProcedures);
            }

            using (var scope = serviceScopeFactory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ILoadingUnitsProvider>().UpdateRange(vertimagConfiguration.LoadingUnits);
            }

            return this.Ok();
        }

        #endregion
    }
}
