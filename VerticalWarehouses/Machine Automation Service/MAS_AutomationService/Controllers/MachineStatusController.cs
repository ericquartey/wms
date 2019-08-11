using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public partial class MachineStatusController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MachineStatusController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        [HttpGet("ExecuteResetSecurity")]
        public void ExecuteResetSecurity()
        {
            this.ExecuteResetSecurity_Method();
        }

        [HttpGet("ExecutePowerOn")]
        public void ExecutePowerOn()
        {
            this.ExecutePowerOn_Method();
        }

        [HttpGet("ExecutePowerOff")]
        public void ExecutePowerOff()
        {
            this.ExecutePowerOff_Method();
        }

        #endregion
    }
}
