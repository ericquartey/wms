using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallationController
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public InstallationController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion
    }
}
