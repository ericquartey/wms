using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

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
            ICalibrateMessageData homingData = new CalibrateMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(homingData, "Execute Homing Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Homing));
        }

        [HttpPost]
        [Route("ExecuteMovement")]
        public void ExecuteMovement([FromBody]MovementMessageDataDTO data)
        {
            var messageData = new MovementMessageData(data);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, "Execute Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Movement));
        }

        [HttpGet("StopCommand")]
        public void StopCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebAPI, MessageType.Stop));
        }

        #endregion
    }
}
