using System;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/setup/[controller]")]
    [ApiController]
    public class DepositAndPickupProcedureController : ControllerBase
    {
        #region Fields

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        //private readonly IElevatorProvider elevatorProvider;

        #region Constructors

        public DepositAndPickupProcedureController(
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            //this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
        }

        #endregion

        //public BayNumber BayNumber { get; set; }

        #region Methods

        [HttpGet("parameters")]
        public ActionResult<RepeatedTestProcedure> GetParameters()
        {
            return this.Ok(this.setupProceduresDataProvider.GetDepositAndPickUpTest());
        }

        [HttpPost("increase-performed-cycles")]
        [Obsolete("This method shall be removed once the test is fully implemented at MissionManager level.")]
        public ActionResult<int> IncreasePerformedCycles()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetDepositAndPickUpTest();

            var procedure = this.setupProceduresDataProvider.IncreasePerformedCycles(procedureParameters);

            return this.Ok(procedure.PerformedCycles);
        }

        #endregion

        //[HttpPost("start/repetitive-horizontal")]
        //[ProducesResponseType(StatusCodes.Status202Accepted)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesDefaultResponseType]
        //public IActionResult StartHorizontalMovements(int bayPositionId, int loadingUnitId)
        //{
        //    this.elevatorProvider.StartRepetitiveHorizontalMovements(bayPositionId, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

        //    return this.Accepted();
        //}

        //[HttpPost("stop/repetitive-horizontal")]
        //[ProducesResponseType(StatusCodes.Status202Accepted)]
        //[ProducesDefaultResponseType]
        //public IActionResult Stop()
        //{
        //    this.elevatorProvider.Stop(this.BayNumber, MessageActor.WebApi);

        //    return this.Accepted();
        //}

        //[HttpPost("stop-test/repetitive-horizontal")]
        //[ProducesResponseType(StatusCodes.Status202Accepted)]
        //[ProducesDefaultResponseType]
        //public IActionResult StopTest()
        //{
        //    this.elevatorProvider.StopTest(this.BayNumber, MessageActor.AutomationService);

        //    return this.Accepted();
        //}
    }
}
