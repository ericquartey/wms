using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class MachineStatusController : BaseAutomationController
    {


        #region Constructors

        public MachineStatusController(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion



        #region Methods

        [HttpPost("power-off")]
        public void PowerOff()
        {
            var powerEnableMessageData = new PowerEnableMessageData(false);

            this.PublishCommand(
                powerEnableMessageData,
                "Power Disable Command",
                MessageActor.FiniteStateMachines,
                MessageType.PowerEnable);
        }

        [HttpPost("power-on")]
        public void PowerOn()
        {
            var powerEnableMessageData = new PowerEnableMessageData(true);

            this.PublishCommand(
                powerEnableMessageData,
                "Power Enable Command",
                MessageActor.FiniteStateMachines,
                MessageType.PowerEnable);
        }

        [HttpPost("reset-security")]
        public void ResetSecurity()
        {
            this.PublishCommand(
                null,
                "Reset Security Command",
                MessageActor.FiniteStateMachines,
                MessageType.ResetSecurity);
        }

        #endregion
    }
}
