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

        [HttpPost("can-move-ext-double")]
        public ActionResult<ActionPolicy> CanMoveExternalDouble(ExternalBayMovementDirection direction, MovementCategory movementCategory, bool isPositionUpper)
        {
            return this.Ok(this.externalBayProvider.CanMoveExtDouble(direction, this.BayNumber, movementCategory, isPositionUpper));
        }

        [HttpPost("find-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            this.externalBayProvider.Homing(Calibration.FindSensor, null, true, false, this.BayNumber, MessageActor.AutomationService);

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
            this.externalBayProvider.Homing(Calibration.ResetEncoder, null, true, false, this.BayNumber, MessageActor.AutomationService);

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

        [HttpPost("move-assisted-ext-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveAssistedExternalBay(ExternalBayMovementDirection direction, bool isPositionUpper)
        {
            this.externalBayProvider.MoveAssistedExtDouble(direction, this.BayNumber, MessageActor.AutomationService, isPositionUpper);

            return this.Accepted();
        }

        [HttpPost("move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManual(ExternalBayMovementDirection direction, bool bypass)
        {
            this.externalBayProvider.MoveManual(direction, -1, null, bypass, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-manual-double")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManualExtDouble(ExternalBayMovementDirection direction, bool bypass)
        {
            this.externalBayProvider.MoveManualExtDouble(direction, -1, null, bypass, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-for-extraction")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MovementForExtraction(bool isUpperPosition)
        {
            this.externalBayProvider.MovementForExtraction(null, this.BayNumber, MessageActor.AutomationService, isUpperPosition);

            return this.Accepted();
        }

        [HttpPost("move-for-insertion")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MovementForInsertion(bool isUpperPosition)
        {
            this.externalBayProvider.MovementForInsertion(this.BayNumber, MessageActor.AutomationService, isUpperPosition);

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

        [HttpPost("start-double-ext-movements")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartDoubleExtBayTest(ExternalBayMovementDirection direction, bool isPositionUpper)
        {
            this.externalBayProvider.StartDoubleExtBayTest(direction, this.BayNumber, MessageActor.AutomationService, isPositionUpper);

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

        [HttpPost("update-cycle")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult UpdateProcedureCycle(int cycle)
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBayExternalCalibration(this.BayNumber);
            this.setupProceduresDataProvider.IncreasePerformedCycles(procedureParameters, cycle);

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

        [HttpPost("update-resolution")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateResolution(double newRace)
        {
            this.baysDataProvider.UpdateResolution(this.BayNumber, newRace);
            return this.Accepted();
        }

        #endregion
    }
}
