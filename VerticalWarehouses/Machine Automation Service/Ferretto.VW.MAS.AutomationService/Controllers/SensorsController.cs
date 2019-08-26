using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.CommonUtils.Messages.Data;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : BaseAutomationController
    {


        #region Constructors

        public SensorsController(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion



        #region Methods

        [HttpGet]
        public ActionResult<SensorsChangedMessageData> Get()
        {
            this.PublishCommand(
                    null,
                    "Sensors changed Command",
                    MessageActor.FiniteStateMachines,
                    MessageType.SensorsChanged,
                    BayIndex.None);

            var messageData = this.WaitForResponseEventAsync<SensorsChangedMessageData>(
                MessageType.SensorsChanged,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting);

            return this.Ok(messageData);
        }

        #endregion
    }
}
