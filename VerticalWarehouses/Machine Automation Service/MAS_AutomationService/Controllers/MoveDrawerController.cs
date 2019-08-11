using System;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class MoveDrawerController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MoveDrawerController(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        [HttpPost("Execute")]
        public void Execute([FromBody]MoveDrawerMessageDataDTO data)
        {
            this.ExecuteMovingDrawer_Method(data);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
            this.Ok();
        }

        private void ExecuteMovingDrawer_Method(MoveDrawerMessageDataDTO data)
        {
            IDrawerOperationMessageData drawerOperationData = new DrawerOperationMessageData(
                data.DrawerOperation,
                DrawerOperationStep.None);
            drawerOperationData.Source = DrawerDestination.InternalBay1Up;
            drawerOperationData.Destination = DrawerDestination.Cell;

            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    drawerOperationData,
                    "Execute Drawer Operation Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.DrawerOperation));
        }

        private void Stop_Method()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Stop Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Stop));
        }

        #endregion
    }
}
