using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerticalOriginProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        #endregion

        #region Constructors

        public VerticalOriginProcedureController(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider)
            : base(eventAggregator)
        {
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet("parameters")]
        public ActionResult<HomingProcedureParameters> GetParameters()
        {
            var axis = this.elevatorDataProvider.GetVerticalAxis();

            var parameters = new HomingProcedureParameters
            {
                UpperBound = axis.UpperBound,
                LowerBound = axis.LowerBound,
                Offset = axis.Offset,
                Resolution = axis.Resolution,
            };

            return this.Ok(parameters);
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Start()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical);

            this.PublishCommand(
                homingData,
                "Execute Homing Command",
                MessageActor.FiniteStateMachines,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
