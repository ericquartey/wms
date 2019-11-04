using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
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
    public class ElevatorController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ElevatorController(
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider,
            IEventAggregator eventAggregator)
        {
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.elevatorWeightCheckProvider = elevatorWeightCheckProvider ?? throw new ArgumentNullException(nameof(elevatorWeightCheckProvider));
            this.eventAggregator = eventAggregator;
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("horizontal/findzero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            var homingData = new HomingMessageData(Axis.Horizontal, Calibration.FindSensor);

            this.PublishCommand(
                homingData,
                "Execute FindZeroSensor Command",
                MessageActor.DeviceManager,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpGet("horizontal/position")]
        public ActionResult<double> GetHorizontalPosition()
        {
            return this.Ok(this.elevatorProvider.HorizontalPosition);
        }

        [HttpGet("vertical/bounds")]
        public ActionResult<AxisBounds> GetVerticalBounds()
        {
            return this.Ok(this.elevatorProvider.GetVerticalBounds());
        }

        [HttpGet("vertical/manual-movements-parameters")]
        public ActionResult<VerticalManualMovementsProcedure> GetVerticalManualMovementsParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetVerticalManualMovements());
        }

        [HttpGet("vertical/offset")]
        public ActionResult<double> GetVerticalOffset()
        {
            return this.Ok(this.elevatorDataProvider.GetAxis(Orientation.Vertical).Offset);
        }

        [HttpGet("vertical/position")]
        public ActionResult<double> GetVerticalPosition()
        {
            return this.Ok(this.elevatorProvider.VerticalPosition);
        }

        [HttpGet("vertical/resolution")]
        public ActionResult<decimal> GetVerticalResolution()
        {
            return this.Ok(this.elevatorDataProvider.GetAxis(Orientation.Vertical).Resolution);
        }

        [HttpPost("horizontal/move-auto")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, int? loadingUnitId, double? loadingUnitGrossWeight)
        {
            // this.elevatorProvider.MoveHorizontalProfileCalibration(direction, this.BayNumber, MessageActor.AutomationService);  // TEST
            this.elevatorProvider.MoveHorizontalAuto(
                direction,
                isStartedOnBoard,
                loadingUnitId,
                loadingUnitGrossWeight,
                false,
                false,
                this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("horizontal/move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalManual(HorizontalMovementDirection direction)
        {
            this.elevatorProvider.MoveHorizontalManual(direction, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("vertical/move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(double targetPosition, double feedRate, bool measure)
        {
            this.elevatorProvider.MoveToVerticalPosition(targetPosition, feedRate, measure, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("vertical/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVertical(VerticalMovementDirection direction)
        {
            this.elevatorProvider.MoveVertical(direction, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("vertical/move-relative")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalOfDistance(double distance)
        {
            this.elevatorProvider.MoveVerticalOfDistance(distance, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("search-horizontal-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SearchHorizontalZero()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Horizontal, Calibration.FindSensor);

            this.PublishCommand(
                homingData,
                "Execute FindZeroSensor Command",
                MessageActor.DeviceManager,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.elevatorProvider.Stop(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("weight-check-stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopWeightCheck()
        {
            this.elevatorWeightCheckProvider.Stop();
            return this.Accepted();
        }

        [HttpPost("vertical/resolution")]
        public IActionResult UpdateVerticalResolution(decimal newResolution)
        {
            this.elevatorDataProvider.UpdateVerticalResolution(newResolution);

            return this.Ok();
        }

        [HttpPost("weight-check")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult WeightCheck(int loadingUnitId, double runToTest, double weight)
        {
            this.elevatorWeightCheckProvider.Start(loadingUnitId, runToTest, weight);

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
