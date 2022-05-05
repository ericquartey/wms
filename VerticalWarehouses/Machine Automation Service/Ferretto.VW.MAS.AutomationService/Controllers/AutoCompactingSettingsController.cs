using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoCompactingSettingsController : ControllerBase
    {
        #region Fields

        private readonly IAutoCompactingSettingsProvider autoCompactingSettingsProvider;

        private readonly ILogger<AutoCompactingSettingsController> logger;

        #endregion

        #region Constructors

        public AutoCompactingSettingsController(
            IAutoCompactingSettingsProvider autoCompactingSettingsProvider,
            ILogger<AutoCompactingSettingsController> logger)
        {
            this.autoCompactingSettingsProvider = autoCompactingSettingsProvider ?? throw new ArgumentNullException(nameof(autoCompactingSettingsProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        [HttpPost("add-or-modify-AutoCompactingSettings")]
        public IActionResult AddAutoCompactingSettings(AutoCompactingSettings autoCompactingSettings)
        {
            this.autoCompactingSettingsProvider.AddOrModifyAutoCompactingSettings(autoCompactingSettings);
            return this.Ok();
        }

        [HttpGet("get-all-AutoCompactingSettings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<AutoCompactingSettings>> GetAllAutoCompactingSettings()
        {
            var autoCompactingSettings = this.autoCompactingSettingsProvider.GetAllAutoCompactingSettings();

            return this.Ok(autoCompactingSettings);
        }

        [HttpPost("remove-AutoCompactingSettings-by-id")]
        public IActionResult RemoveAutoCompactingSettingsById(int id)
        {
            this.autoCompactingSettingsProvider.RemoveAutoCompactingSettingsById(id);
            return this.Ok();
        }

        #endregion
    }
}
