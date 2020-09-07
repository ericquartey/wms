using System;
using Ferretto.VW.MAS.SocketLink.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/socket-link")]
    [ApiController]
    public class SocketLinkController : ControllerBase
    {
        #region Fields

        private readonly ISystemSocketLinkProvider systemSocketLinkProvider;

        #endregion

        #region Constructors

        public SocketLinkController(ISystemSocketLinkProvider systemSocketLinkProvider)
        {
            this.systemSocketLinkProvider = systemSocketLinkProvider ?? throw new ArgumentNullException(nameof(systemSocketLinkProvider));
        }

        #endregion

        #region Methods

        [HttpPut("socketlink-set-enable")]
        public IActionResult SocketLinkSetEnableSync(bool isEnabled)
        {
            this.systemSocketLinkProvider.CanEnableSyncMode = isEnabled;
            return this.Ok();
        }

        #endregion
    }
}
