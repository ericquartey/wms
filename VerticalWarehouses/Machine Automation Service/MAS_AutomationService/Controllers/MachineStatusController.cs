using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public partial class MachineStatusController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public MachineStatusController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpGet("ExecutePowerOff")]
        public void ExecutePowerOff()
        {
            this.ExecutePowerOff_Method();
        }

        [HttpGet("ExecutePowerOn")]
        public void ExecutePowerOn()
        {
            this.ExecutePowerOn_Method();
        }

        [HttpGet("ExecuteResetSecurity")]
        public void ExecuteResetSecurity()
        {
            this.ExecuteResetSecurity_Method();
        }

        #endregion
    }
}
