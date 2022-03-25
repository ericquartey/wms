using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShuttersController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IShutterProvider shutterProvider;

        #endregion

        #region Constructors

        public ShuttersController(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IBaysDataProvider baysDataProvider,
            IShutterProvider shutterProvider,
            ISensorsProvider sensorsProvider)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.sensorsProvider = sensorsProvider ?? throw new ArgumentNullException(nameof(sensorsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.shutterProvider = shutterProvider ?? throw new ArgumentNullException(nameof(shutterProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet("position")]
        public ActionResult<ShutterPosition> GetShutterPosition()
        {
            var index = this.baysDataProvider.GetShutterInverterIndex(this.BayNumber);
            return this.Ok(this.sensorsProvider.GetShutterPosition(index));
        }

        [HttpGet("test-parameters")]
        public ActionResult<RepeatedTestProcedure> GetTestParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetBayShutterTest(this.BayNumber));
        }

        [HttpPost("move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Move(ShutterMovementDirection direction)
        {
            this.shutterProvider.Move(direction, true, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("moveTo")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveTo(ShutterPosition targetPosition)
        {
            this.shutterProvider.MoveTo(targetPosition, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("moveToBayNumber")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToBayNumber(ShutterPosition targetPosition, BayNumber bayNumber)
        {
            this.shutterProvider.MoveTo(targetPosition, bayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("reset-test")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ResetTest()
        {
            this.shutterProvider.ResetTest(this.BayNumber);

            return this.Accepted();
        }

        [HttpPost("run-test")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult RunTest(int delayInSeconds, int testCycleCount)
        {
            this.shutterProvider.RunTest(delayInSeconds, testCycleCount, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.shutterProvider.Stop(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        #endregion
    }
}
