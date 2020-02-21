using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.LaserDriver;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class MissionOperationsController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IMissionOperationsProvider missionOperationsProvider;

        private readonly IMissionOperationsWmsWebService missionOperationsWmsWebService;

        private readonly IMissionsWmsWebService missionsWmsWebService;

        #endregion

        #region Constructors

        public MissionOperationsController(
            IMissionOperationsProvider missionOperationsProvider,
            IMissionOperationsWmsWebService missionOperationsWmsWebService,
            IMissionsWmsWebService missionsWmsWebService,
            IBaysDataProvider baysDataProvider)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.missionOperationsProvider = missionOperationsProvider ?? throw new ArgumentNullException(nameof(missionOperationsProvider));
            this.missionOperationsWmsWebService = missionOperationsWmsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWmsWebService));
            this.missionsWmsWebService = missionsWmsWebService ?? throw new ArgumentNullException(nameof(missionsWmsWebService));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpPost("{id}/abort")]
        public async Task<ActionResult> AbortAsync(int id)
        {
            await this.missionOperationsProvider.AbortAsync(id);

            return this.Ok();
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAsync(int id, double quantity, string printerName)
        {
            await this.missionOperationsProvider.CompleteAsync(id, quantity, printerName);

            return this.Ok();
        }

        [HttpPost("{id}/execute")]
        public async Task<ActionResult<MissionOperation>> ExecuteAsync(int id, [FromServices] ILaserProvider laserProvider)
        {
            if (laserProvider is null)
            {
                throw new ArgumentNullException(nameof(laserProvider));
            }

            var operation = await this.missionOperationsWmsWebService.ExecuteAsync(id);
            var mission = await this.missionsWmsWebService.GetDetailsByIdAsync(operation.MissionId);

            var compartment = mission.LoadingUnit?.Compartments?.SingleOrDefault(c => c.Id == operation.CompartmentId);
            if (compartment != null && compartment.XPosition.HasValue && compartment.YPosition.HasValue)
            {
                var laserOriginX = mission.LoadingUnit.Width / 2;
                var laserOriginY = mission.LoadingUnit.Depth / 2;

                var compartmentX = compartment.XPosition.Value;
                var compartmentY = compartment.YPosition.Value;

                var bay = this.baysDataProvider.GetByNumber(this.BayNumber);

                if (bay.Side is DataModels.WarehouseSide.Back)
                {
                    compartmentX = mission.LoadingUnit.Width - compartmentX;
                    compartmentY = mission.LoadingUnit.Depth - compartmentY;
                }

                // x coordinate is flipped
                var compartmentLaserX = -(compartmentX - laserOriginX);
                var compartmentLaserY = compartmentY - laserOriginY;

                laserProvider.MoveToPositionAndSwitchOn(this.BayNumber, compartmentLaserX, compartmentLaserY);
            }

            return this.Ok(operation);
        }

        [HttpGet("count")]
        public ActionResult<int> GetByBayCount()
        {
            var missionOperationsCount = this.missionOperationsProvider.GetCountByBay(this.BayNumber);

            return this.Ok(missionOperationsCount);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MissionOperation>> GetByIdAsync(int id)
        {
            return this.Ok(await this.missionOperationsWmsWebService.GetByIdAsync(id));
        }

        [HttpPost("{id}/partially-complete")]
        public async Task<ActionResult> PartiallyCompleteAsync(int id, double quantity, string printerName)
        {
            await this.missionOperationsProvider.PartiallyCompleteAsync(id, quantity, printerName);

            return this.Ok();
        }

        #endregion
    }
}
