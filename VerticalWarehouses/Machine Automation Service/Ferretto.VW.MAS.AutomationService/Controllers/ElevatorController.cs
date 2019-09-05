using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : BaseAutomationController
    {
        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        private readonly IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        #endregion

        #region Constructors

        public ElevatorController(
            IEventAggregator eventAggregator,
            IElevatorProvider elevatorProvider,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider)
            : base(eventAggregator)
        {
            if (elevatorProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorProvider));
            }

            if (installationHub is null)
            {
                throw new ArgumentNullException(nameof(installationHub));
            }

            if (elevatorWeightCheckProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorWeightCheckProvider));
            }

            this.elevatorProvider = elevatorProvider;
            this.installationHub = installationHub;
            this.elevatorWeightCheckProvider = elevatorWeightCheckProvider;
        }

        #endregion

        #region Methods

        [HttpGet("horizontal/position")]
        public ActionResult<decimal> GetHorizontalPosition()
        {
            try
            {
                var position = this.elevatorProvider.GetHorizontalPosition();
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<decimal>(ex);
            }
        }

        [HttpGet("vertical/position")]
        public ActionResult<decimal> GetVerticalPosition()
        {
            try
            {
                var position = this.elevatorProvider.GetVerticalPosition();
                return this.Ok(position);
            }
            catch (Exception ex)
            {
                return this.NegativeResponse<decimal>(ex);
            }
        }

        [HttpPost("horizontal/move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontal(HorizontalMovementDirection direction)
        {
            try
            {
                this.elevatorProvider.MoveHorizontal(direction);
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
        public IActionResult MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory)
        {
            try
            {
                this.elevatorProvider.MoveToVerticalPosition(targetPosition, feedRateCategory);
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
                this.elevatorProvider.MoveVertical(direction);
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
        public IActionResult MoveVerticalOfDistance(decimal distance)
        {
            try
            {
                this.elevatorProvider.MoveVerticalOfDistance(distance);
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
                this.elevatorProvider.Stop();
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
        public IActionResult UpdateResolution(decimal newResolution)
        {
            try
            {
                this.elevatorProvider.UpdateResolution(newResolution);

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
        public IActionResult WeightCheck(int loadingUnitId, decimal runToTest, decimal weight)
        {
            try
            {
                this.elevatorWeightCheckProvider.Start(loadingUnitId, runToTest, weight);

                var data = new ElevatorWeightCheckMessageData() { Weight = 200 };
                this.installationHub.Clients.All.ElevatorWeightCheck(new NotificationMessageUI<IElevatorWeightCheckMessageData>() { Data = data });

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
