using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Data;

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
                    MessageType.SensorsChanged);

            var messageData = this.WaitForResponseEventAsync<SensorsChangedMessageData>(
                MessageType.SensorsChanged,
                MessageActor.FiniteStateMachines,
                MessageStatus.OperationExecuting);

            return this.Ok(messageData);
        }

        #endregion
    }
}
