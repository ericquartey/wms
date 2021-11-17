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
    public class LogoutSettingsController : ControllerBase
    {
        #region Fields

        private readonly ILogger<LogoutSettingsController> logger;

        private readonly ILogoutSettingsProvider logoutSettingsProvider;

        #endregion

        #region Constructors

        public LogoutSettingsController(
            ILogoutSettingsProvider logoutSettingsProvider,
            ILogger<LogoutSettingsController> logger)
        {
            this.logoutSettingsProvider = logoutSettingsProvider ?? throw new ArgumentNullException(nameof(logoutSettingsProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        [HttpPost("add-or-modify-logoutsettings")]
        public IActionResult AddLogoutSettings(LogoutSettings logoutSettings)
        {
            this.logoutSettingsProvider.AddOrModifyLogoutSettings(logoutSettings);
            return this.Ok();
        }

        [HttpPost("remove-logoutsettings-by-id")]
        public IActionResult RemoveLogoutSettingsById(int id)
        {
            this.logoutSettingsProvider.RemoveLogoutSettingsById(id);
            return this.Ok();
        }

        [HttpGet("get-all-logoutsettings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<LogoutSettings>> GetAllLogoutSettings()
        {
            var logoutsettings = this.logoutSettingsProvider.GetAllLogoutSettings();

            return this.Ok(logoutsettings);
        }

        #endregion
    }
}
