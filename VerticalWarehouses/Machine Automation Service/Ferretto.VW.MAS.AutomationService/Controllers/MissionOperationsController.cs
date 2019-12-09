using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionOperationsController : ControllerBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILogger logger;

        private readonly IMissionOperationsProvider missionOperationsProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionOperationsController(
            ILogger<MissionOperationsController> logger,
            IMissionsDataProvider missionsDataProvider,
            IMissionOperationsProvider missionOperationsProvider,
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.missionOperationsProvider = missionOperationsProvider ?? throw new ArgumentNullException(nameof(missionOperationsProvider));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
        }

        #endregion

        #region Methods

        [HttpPost("{id}/abort")]
        public async Task<ActionResult> AbortAsync(int id)
        {
            await this.missionOperationsProvider.AbortAsync(id);

            return this.Ok();
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAsync(int id, double quantity)
        {
            await this.missionOperationsProvider.CompleteAsync(id, quantity);

            return this.Ok();
        }

        [HttpPost("reset-machine")]
        [Obsolete("Vorrei capire se e' il punto giusto dove mettere il metodo, per oggi la lascio qui")]
        public ActionResult ResetMachine()
        {
            this.baysDataProvider.ResetMachine();
            this.elevatorDataProvider.ResetMachine();
            this.missionsDataProvider.ResetMachine();

            this.logger.LogInformation($"RESET MACHINE.");

            return this.Ok();
        }

        #endregion
    }
}
