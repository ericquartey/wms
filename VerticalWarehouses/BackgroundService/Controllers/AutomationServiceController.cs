using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BackgroundService
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomationServiceController
    {
        #region Fields

        private readonly IHubContext<AutomationServiceHub, IAutomationServiceHub> hub;

        #endregion

        #region Constructors

        public AutomationServiceController(IHubContext<AutomationServiceHub, IAutomationServiceHub> hub)
        {
            this.hub = hub;
        }

        #endregion

        #region Methods

        [HttpGet("test-method")]
        public string TestMethod()
        {
            return "test-method executed.\n";
        }

        #endregion
    }
}
