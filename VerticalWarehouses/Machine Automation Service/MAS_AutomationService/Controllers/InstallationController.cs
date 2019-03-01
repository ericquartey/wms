using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.Common_Utils.Messages.Data;

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

        #region Methods

        [HttpGet("ExecuteHoming")]
        public void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Execute Homing Command", MessageActor.WebAPI, MessageActor.FiniteStateMachines, MessageType.Homing));
        }

        [HttpPost("ExecuteMovement")]
        public void ExecuteRelativeHorizontalMovement([FromBody] decimal mm, [FromBody] int axis, [FromBody] int movementType, [FromBody] uint speedPercentage)
        {
            var data = new MovementMessageData(mm, axis, movementType, speedPercentage);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(data, "Execute Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Movement));
        }

        [HttpGet("StopCommand")]
        public void StopCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Stop));
        }

        #endregion
    }
}
