using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public partial class MachineServiceController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MachineServiceController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200)]
        [HttpGet("ExecuteMachineReset")]
        public void ExecuteMachineReset(ResetOperation operation)
        {
            this.ExecuteMachineReset_Method(operation);
        }

        #endregion
    }
}
