using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class MoveDrawerController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public MoveDrawerController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            this.eventAggregator = eventAggregator;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200)]
        [HttpGet("Execute")]
        public void Execute()
        {
            this.ExecuteMovingDrawer_Method();
            this.Ok();
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            //TODO: Publish a command message to stop operation
            this.Ok();
        }

        private void ExecuteMovingDrawer_Method()
        {
            IDrawerOperationMessageData drawerOperationData = new DrawerOperationMessageData(
                DrawerOperation.ManualStore,
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

        #endregion
    }
}
