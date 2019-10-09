using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PowerController : BaseAutomationController
    {
        #region Constructors

        public PowerController(IEventAggregator eventAggregator)
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
                MessageActor.AutomationService,
                MessageType.PowerEnable);
        }

        [HttpPost("power-on")]
        public void PowerOn()
        {
            var powerEnableMessageData = new PowerEnableMessageData(true);

            this.PublishCommand(
                powerEnableMessageData,
                "Power Enable Command",
                MessageActor.AutomationService,
                MessageType.PowerEnable);
        }

        #endregion
    }
}
