using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class VerticalOriginProcedureController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ILogger<VerticalOriginProcedureController> logger;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public VerticalOriginProcedureController(
            IElevatorDataProvider elevatorDataProvider,
            IElevatorProvider elevatorProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILogger<VerticalOriginProcedureController> logger
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
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
            this.logger.LogDebug($"Start Homing from UI");
            this.elevatorProvider.Homing(Axis.HorizontalAndVertical,
                Calibration.FindSensor,
                loadUnitId: null,
                showErrors: true,
                this.BayNumber,
                MessageActor.WebApi);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.logger.LogDebug($"Stop elevator from UI");
            this.elevatorProvider.Stop(this.BayNumber, MessageActor.WebApi);

            return this.Accepted();
        }

        #endregion
    }
}
