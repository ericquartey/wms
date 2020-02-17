using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;


namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class VerticalOriginProcedureController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public VerticalOriginProcedureController(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.eventAggregator = eventAggregator;
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet("parameters")]
        public ActionResult<HomingProcedureParameters> GetParameters()
        {
            var axis = this.elevatorDataProvider.GetAxis(DataModels.Orientation.Vertical);

            var procedureParameters = this.setupProceduresDataProvider.GetVerticalOriginCalibration();

            var parameters = new HomingProcedureParameters
            {
                UpperBound = axis.UpperBound,
                LowerBound = axis.LowerBound,
                Offset = axis.Offset,
                Resolution = axis.Resolution,
                IsCompleted = procedureParameters.IsCompleted,
            };

            return this.Ok(parameters);
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Start()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.HorizontalAndVertical, Calibration.FindSensor, null, true);

            this.PublishCommand(
                homingData,
                "Execute Homing Command",
                MessageActor.DeviceManager,
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
                MessageActor.DeviceManager,
                MessageType.Stop);

            return this.Accepted();
        }

        [Obsolete("Move message publishing to providers.")]
        protected void PublishCommand(
            IMessageData messageData,
            string description,
            MessageActor receiver,
            MessageType messageType)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        receiver,
                        MessageActor.WebApi,
                        messageType,
                        this.BayNumber));
        }

        #endregion
    }
}
