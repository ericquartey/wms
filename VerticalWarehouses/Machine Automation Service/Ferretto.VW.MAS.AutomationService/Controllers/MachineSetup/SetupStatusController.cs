using Ferretto.VW.MAS.DataLayer;
using Microsoft.AspNetCore.Mvc;


namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class SetupStatusController : ControllerBase
    {
        #region Fields

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public SetupStatusController(ISetupStatusProvider setupStatusProvider)
        {
            this.setupStatusProvider = setupStatusProvider ?? throw new System.ArgumentNullException(nameof(setupStatusProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<SetupStatusCapabilities> Get()
        {
            return this.Ok(this.setupStatusProvider.Get());
        }

        #endregion
    }
}
