using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;
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

        [HttpGet("horizontal/position")]
        public ActionResult<double> GetHorizontalPosition()
        {
            try
            {
                var position = this.elevatorProvider.GetHorizontalPosition(this.BayNumber);
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<double>(ex);
            }
        }

        [HttpGet("vertical/position")]
        public ActionResult<double> GetVerticalPosition()
        {
            try
            {
                var position = this.elevatorProvider.GetVerticalPosition(this.BayNumber);
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<double>(ex);
            }
        }

        [HttpPost("horizontal/move-auto")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, int? LoadingUnitId, double? loadingUnitGrossWeight)
        {
            try
            {
                this.elevatorProvider.MoveHorizontalAuto(direction, isStartedOnBoard, LoadingUnitId, loadingUnitGrossWeight, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("horizontal/move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalManual(HorizontalMovementDirection direction)
        {
            try
            {
                this.elevatorProvider.MoveHorizontalManual(direction, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(double targetPosition, FeedRateCategory feedRateCategory)
        {
            try
            {
                this.elevatorProvider.MoveToVerticalPosition(targetPosition, feedRateCategory, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVertical(VerticalMovementDirection direction)
        {
            try
            {
                this.elevatorProvider.MoveVertical(direction, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/move-relative")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalOfDistance(double distance)
        {
            try
            {
                this.elevatorProvider.MoveVerticalOfDistance(distance, this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            try
            {
                this.elevatorProvider.Stop(this.BayNumber);
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("weight-check-stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopWeightCheck()
        {
            try
            {
                this.elevatorWeightCheckProvider.Stop();
                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("vertical/resolution")]
        public IActionResult UpdateVerticalResolution(decimal newResolution)
        {
            try
            {
                this.elevatorDataProvider.UpdateVerticalResolution(newResolution);

                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("weight-check")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult WeightCheck(int loadingUnitId, double runToTest, double weight)
        {
            try
            {
                this.elevatorWeightCheckProvider.Start(loadingUnitId, runToTest, weight);

                var data = new ElevatorWeightCheckMessageData() { Weight = 200 };

                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        #endregion
    }
}
