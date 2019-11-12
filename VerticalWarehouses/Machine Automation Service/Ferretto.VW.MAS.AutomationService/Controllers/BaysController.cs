using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaysController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public BaysController(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IBaysDataProvider baysProvider,
            IEventAggregator eventAggregator)
        {
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> ActivateAsync()
        {
            var bay = this.baysProvider.Activate(this.BayNumber);

            return this.Ok(bay);
        }

        [HttpPost("deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> DeactivateAsync()
        {
            var bay = this.baysProvider.Deactivate(this.BayNumber);

            return this.Ok(bay);
        }

        [HttpPost("findzero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, Calibration.FindSensor);

            this.PublishCommand(
                homingData,
                "Execute FindZeroSensor Command",
                MessageActor.DeviceManager,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<Bay>> GetAll()
        {
            var bay = this.baysProvider.GetAll();

            return this.Ok(bay);
        }

        [HttpGet("{bayNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> GetByNumber(BayNumber bayNumber)
        {
            var bay = this.baysProvider.GetByNumber(bayNumber);

            return this.Ok(bay);
        }

        [HttpGet("height-check-parameters")]
        public ActionResult<PositioningProcedure> GetHeightCheckParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetBayHeightCheck());
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.BayChain, Calibration.ResetEncoder);

            this.PublishCommand(
                homingData,
                "Execute Homing Command",
                MessageActor.DeviceManager,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult Move(HorizontalMovementDirection direction)
        {
            throw new NotImplementedException();
        }

        [HttpPost("height")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<Bay> UpdateHeightAsync(int position, double height)
        {
            var bay = this.baysProvider.UpdatePosition(this.BayNumber, position, height);

            return this.Ok(bay);
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
