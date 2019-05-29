using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController :
        BaseController,
        IReadAllPagedController<Mission>,
        IReadSingleController<Mission, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMissionProvider missionProvider;

        private readonly ISchedulerService schedulerService;

        #endregion

        #region Constructors

        public MissionsController(
            ILogger<MissionsController> logger,
            IHubContext<DataHub, IDataHub> hubContext,
            IMissionProvider missionProvider,
            ISchedulerService schedulerService)
            : base(hubContext)
        {
            this.logger = logger;
            this.missionProvider = missionProvider;
            this.schedulerService = schedulerService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(Mission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("{id}/abort")]
        public Task<ActionResult<Mission>> AbortAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        [ProducesResponseType(typeof(MissionExecution), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/complete/{quantity}")]
        public async Task<ActionResult<MissionExecution>> CompleteItemAsync(int id, double quantity)
        {
            var result = await this.schedulerService.CompleteItemMissionAsync(id, quantity);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.NotifyMissionUpdateAsync(result.Entity);

            var updatedMission = await this.missionProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(MissionExecution), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<MissionExecution>> CompleteLoadingUnitAsync(int id)
        {
            var result = await this.schedulerService.CompleteLoadingUnitMissionAsync(id);
            if (!result.Success)
            {
               return this.NegativeResponse(result);
            }

            await this.NotifyMissionUpdateAsync(result.Entity);

            var updatedMission = await this.missionProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(MissionExecution), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<Mission>> ExecuteAsync(int id)
        {
            var result = await this.schedulerService.ExecuteMissionAsync(id);
            if (!result.Success)
            {
               return this.NegativeResponse(result);
            }

            await this.NotifyMissionUpdateAsync(result.Entity);

            var updatedMission = await this.missionProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(IEnumerable<Mission>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mission>>> GetAllAsync(
            int skip = 0,
            int take = 0,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(
                    await this.missionProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                return await this.missionProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(Mission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Mission>> GetByIdAsync(int id)
        {
            var result = await this.missionProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(MissionDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}/details")]
        public async Task<ActionResult<MissionDetails>> GetDetailsByIdAsync(int id)
        {
            var result = await this.missionProvider.GetDetailsByIdAsync(id);
            return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            try
            {
                return this.Ok(await this.missionProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        private async Task NotifyMissionUpdateAsync(MissionExecution mission)
        {
            await this.NotifyEntityUpdatedAsync(nameof(Mission), mission.Id, HubEntityOperation.Updated);
            if (mission.ItemListRowId != null)
            {
                await this.NotifyEntityUpdatedAsync(nameof(ItemListRow), mission.ItemListRowId, HubEntityOperation.Updated);
                await this.NotifyEntityUpdatedAsync(nameof(ItemList), mission.ItemListId, HubEntityOperation.Updated);
            }

            if (mission.CompartmentId != null)
            {
                await this.NotifyEntityUpdatedAsync(nameof(Compartment), mission.CompartmentId, HubEntityOperation.Updated);
            }

            if (mission.LoadingUnitId != null)
            {
                await this.NotifyEntityUpdatedAsync(nameof(LoadingUnit), mission.LoadingUnitId, HubEntityOperation.Updated);
            }

            if (mission.ItemId != null)
            {
                await this.NotifyEntityUpdatedAsync(nameof(Item), mission.ItemId, HubEntityOperation.Updated);
            }
        }

        #endregion
    }
}
