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

        #region Constructors

        public DepositAndPickupProcedureController(
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

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
    }
}
