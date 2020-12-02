using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILogger<MissionOperationsController> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionsController(IMissionsDataProvider missionsDataProvider,
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            ILogger<MissionOperationsController> logger
            )
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult<IEnumerable<Mission>> GetAll()
        {
            var missions = this.missionsDataProvider.GetAllActiveMissions();

            return this.Ok(missions);
        }

        [HttpGet("active/unit")]
        public ActionResult<List<int>> GetAllUnitGoBay()
        {
            var missions = this.missionsDataProvider.GetAllActiveUnitGoBay();
            return this.Ok(missions);
        }

        [HttpGet("active/unit/by/bay")]
        public ActionResult<List<int>> GetAllUnitGoBay(BayNumber bayNumber)
        {
            var missions = this.missionsDataProvider.GetAllActiveUnitGoBay(bayNumber);
            return this.Ok(missions);
        }

        [HttpGet("active/unit/cell")]
        public ActionResult<List<int>> GetAllUnitGoCell()
        {
            var missions = this.missionsDataProvider.GetAllActiveUnitGoCell();
            return this.Ok(missions);
        }

        [HttpGet("active/unit/cell/by/bay")]
        public ActionResult<List<int>> GetAllUnitGoCell(BayNumber bayNumber)
        {
            var missions = this.missionsDataProvider.GetAllActiveUnitGoCell(bayNumber);
            return this.Ok(missions);
        }

        [HttpGet("{id}/wms")]
        public async Task<ActionResult<WMS.Data.WebAPI.Contracts.MissionInfo>> GetByWmsIdAsync(int id, [FromServices] WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService)
        {
            if (missionsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(missionsWmsWebService));
            }

            return this.Ok(await missionsWmsWebService.GetByIdAsync(id));
        }

        [HttpGet("{id}/wms/details")]
        public async Task<ActionResult<WMS.Data.WebAPI.Contracts.MissionWithLoadingUnitDetails>> GetWmsDetailsByIdAsync(
            int id,
            [FromServices] WMS.Data.WebAPI.Contracts.IMissionsWmsWebService missionsWmsWebService)
        {
            if (missionsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(missionsWmsWebService));
            }

            return this.Ok(await missionsWmsWebService.GetDetailsByIdAsync(id));
        }

        [HttpPost("reset-machine")]
        public ActionResult ResetMachine()
        {
            this.baysDataProvider.ResetMachine();
            this.elevatorDataProvider.ResetMachine();
            this.missionsDataProvider.ResetMachine(MessageActor.AutomationService);

            this.logger.LogInformation($"RESET MACHINE.");

            return this.Ok();
        }

        #endregion
    }
}
