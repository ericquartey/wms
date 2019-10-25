﻿using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;

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
            var configuration = new VertimagConfiguration()
            {
                Machine = this.machineProvider.Get(),
                SetupProcedures = this.setupProceduresDataProvider.GetAll(),
                LoadingUnits = this.loadingUnitsProvider.GetAll(),
            };

            return this.Ok(configuration);
        }

        [HttpPost]
        public IActionResult Set(VertimagConfiguration vertimagConfiguration)
        {
            this.machineProvider.Update(vertimagConfiguration.Machine);
            this.setupProceduresDataProvider.Update(vertimagConfiguration.SetupProcedures);

            return this.Ok();
        }

        #endregion
    }
}
