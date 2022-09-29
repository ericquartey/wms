using System;
using System.Security.Cryptography;
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
    public class CarouselController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICarouselProvider carouselProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public CarouselController(ICarouselProvider carouselProvider,
            IBaysDataProvider baysDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.carouselProvider = carouselProvider ?? throw new ArgumentNullException(nameof(carouselProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("can-move")]
        public ActionResult<ActionPolicy> CanMove(VerticalMovementDirection direction, MovementCategory movementCategory)
        {
            var bay = this.baysDataProvider.GetByNumber(this.BayNumber);
            return this.Ok(this.carouselProvider.CanMove(direction, bay, movementCategory));
        }

        [HttpPost("bay/find-lost-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindLostZero()
        {
            this.carouselProvider.MoveFindZero(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("find-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            this.carouselProvider.Homing(Calibration.FindSensor, null, true, this.BayNumber, MessageActor.AutomationService, false);

            return this.Accepted();
        }

        [HttpGet("parameters")]
        public ActionResult<RepeatedTestProcedure> GetParameters()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBayCarouselCalibration(this.BayNumber);

            return this.Ok(procedureParameters);
        }

        [HttpGet("position")]
        public ActionResult<double> GetPosition()
        {
            return this.Ok(this.carouselProvider.GetPosition(this.BayNumber));
        }

        [HttpPost("homing")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Homing(bool bypassSensor)
        {
            this.carouselProvider.Homing(Calibration.ResetEncoder, null, true, this.BayNumber, MessageActor.AutomationService, bypassSensor);

            return this.Accepted();
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult Move(VerticalMovementDirection direction)
        {
            var bay = this.baysDataProvider.GetByNumber(this.BayNumber);
            this.carouselProvider.Move(direction, null, bay, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-assisted")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveAssisted(VerticalMovementDirection direction)
        {
            this.carouselProvider.MoveAssisted(direction, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult MoveManual(VerticalMovementDirection direction)
        {
            var bay = this.baysDataProvider.GetByNumber(this.BayNumber);
            this.carouselProvider.MoveManual(direction, -1, null, true, bay, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("reset-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ResetCalibration()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetBayCarouselCalibration(this.BayNumber);
            this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters);

            return this.Accepted();
        }

        [HttpPost("set-completed")]
        public IActionResult SetCalibrationCompleted()
        {
            this.setupProceduresDataProvider.MarkAsCompleted(this.setupProceduresDataProvider.GetBayCarouselCalibration(this.BayNumber), false);
            return this.Ok();
        }

        [HttpPost("start-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartCalibration()
        {
            this.carouselProvider.StartTest(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.carouselProvider.Stop(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("stop-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopCalibration()
        {
            this.carouselProvider.StopTest(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("update-elevator-distance")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult UpdateElevatorChainDistance(double value)
        {
            this.baysDataProvider.UpdateELevatorDistance(this.BayNumber, value);
            return this.Accepted();
        }

        #endregion
    }
}
