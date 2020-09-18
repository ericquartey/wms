using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalBayController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IExternalBayProvider externalBayProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ExternalBayController(IExternalBayProvider externalBayProvider,
            IBaysDataProvider baysDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.externalBayProvider = externalBayProvider ?? throw new ArgumentNullException(nameof(externalBayProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("can-move")]
        public ActionResult<ActionPolicy> CanMove(ExternalBayMovementDirection direction, MovementCategory movementCategory)
        {
            return this.Ok(this.externalBayProvider.CanMove(direction, this.BayNumber, movementCategory));
        }

        [HttpPost("find-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            this.externalBayProvider.Homing(Calibration.FindSensor, null, true, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpGet("parameters")]
        public ActionResult<RepeatedTestProcedure> GetParameters()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBayExternalCalibration(this.BayNumber);

            return this.Ok(procedureParameters);
        }

        [HttpGet("position")]
        public ActionResult<double> GetPosition()
        {
            return this.Ok(this.externalBayProvider.GetPosition(this.BayNumber));
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing()
        {
            this.externalBayProvider.Homing(Calibration.ResetEncoder, null, true, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult Move(ExternalBayMovementDirection direction)
        {
            this.externalBayProvider.Move(direction, null, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-assisted")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveAssisted(ExternalBayMovementDirection direction)
        {
            this.externalBayProvider.MoveAssisted(direction, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManual(ExternalBayMovementDirection direction)
        {
            this.externalBayProvider.MoveManual(direction, -1, null, true, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-for-extraction")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MovementForExtraction()
        {
            this.externalBayProvider.MovementForExtraction(null, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-for-insertion")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MovementForInsertion()
        {
            this.externalBayProvider.MovementForInsertion(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("reset-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ResetCalibration()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBayExternalCalibration(this.BayNumber);
            this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters);

            return this.Accepted();
        }

        [HttpPost("set-completed")]
        public IActionResult SetCalibrationCompleted()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayExternalCalibration(this.BayNumber), false);
            return this.Ok();
        }

        [HttpPost("start-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartCalibration()
        {
            this.externalBayProvider.StartTest(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.externalBayProvider.Stop(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopCalibration()
        {
            this.externalBayProvider.StopTest(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("update-extra-race-distance")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateExtraRaceDistance(double value)
        {
            this.baysDataProvider.UpdateExtraRace(this.BayNumber, value);
            return this.Accepted();
        }

        [HttpPost("update-race-distance")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateRaceDistance(double value)
        {
            this.baysDataProvider.UpdateRace(this.BayNumber, value);
            return this.Accepted();
        }

        #endregion
    }
}
