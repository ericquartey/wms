using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.FiniteStateMachines.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : BaseAutomationController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public ElevatorController(
            IEventAggregator eventAggregator,
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineProvider machineProvider,
            IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider)
            : base(eventAggregator)
        {
            if (elevatorProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorProvider));
            }

            if (elevatorDataProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorDataProvider));
            }

            if (elevatorWeightCheckProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorWeightCheckProvider));
            }
            if (machineProvider is null)
            {
                throw new ArgumentNullException(nameof(machineProvider));
            }

            this.elevatorProvider = elevatorProvider;
            this.elevatorDataProvider = elevatorDataProvider;
            this.elevatorWeightCheckProvider = elevatorWeightCheckProvider;
            this.machineProvider = machineProvider;
        }

        #endregion

        #region Methods

        [HttpPost("horizontal/findzero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Horizontal, Calibration.FindSensor);

            this.PublishCommand(
                homingData,
                "Execute FindZeroSensor Command",
                MessageActor.FiniteStateMachines,
                MessageType.Homing);

            return this.Accepted();
        }

        [HttpGet("horizontal/position")]
        public ActionResult<double> GetHorizontalPosition()
        {
            return this.Ok(this.elevatorProvider.HorizontalPosition);
        }

        [HttpGet("vertical/position")]
        public ActionResult<double> GetVerticalPosition()
        {
            return this.Ok(this.elevatorProvider.VerticalPosition);
        }

        [HttpPost("horizontal/move-auto")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, int? LoadingUnitId, double? loadingUnitGrossWeight)
        {
            this.elevatorProvider.MoveHorizontalAuto(direction, isStartedOnBoard, LoadingUnitId, loadingUnitGrossWeight, this.BayNumber);
            return this.Accepted();
        }

        [HttpPost("horizontal/move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalManual(HorizontalMovementDirection direction)
        {
            this.elevatorProvider.MoveHorizontalManual(direction, this.BayNumber);
            return this.Accepted();
        }

        [HttpPost("vertical/move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(double targetPosition, double feedRate)
        {
            this.elevatorProvider.MoveToVerticalPosition(targetPosition, feedRate, this.BayNumber);

            return this.Accepted();
        }

        [HttpPost("vertical/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVertical(VerticalMovementDirection direction)
        {
            this.elevatorProvider.MoveVertical(direction, this.BayNumber);
            return this.Accepted();
        }

        [HttpPost("vertical/move-relative")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalOfDistance(double distance)
        {
            this.elevatorProvider.MoveVerticalOfDistance(distance, this.BayNumber);
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.elevatorProvider.Stop(this.BayNumber);
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

            var data = new ElevatorWeightCheckMessageData() { Weight = 200 };

            return this.Accepted();
        }

        #endregion
    }
}
