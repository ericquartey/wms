using System;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class HomingController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public HomingController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpGet("Execute")]
        public void Execute()
        {
            this.ExecuteHoming_Method();
        }

        private void ExecuteHoming_Method()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    homingData,
                    "Execute Homing Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Homing));
        }

        #endregion
    }
}
