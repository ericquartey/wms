using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController :
        ControllerBase,
        IReadAllPagedController<Mission>,
        IReadSingleController<Mission, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMissionProvider missionProvider;

        private readonly IHubContext<SchedulerHub, ISchedulerHub> schedulerHubContext;

        private readonly ISchedulerService schedulerService;

        #endregion

        #region Constructors

        public MissionsController(
            ILogger<MissionsController> logger,
            IMissionProvider missionProvider,
            ISchedulerService schedulerService,
            IHubContext<SchedulerHub, ISchedulerHub> schedulerHubContext)
        {
            this.logger = logger;
            this.missionProvider = missionProvider;
            this.schedulerService = schedulerService;
            this.schedulerHubContext = schedulerHubContext;
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

        [ProducesResponseType(typeof(Mission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<Mission>> CompleteAsync(int id)
        {
            var result = await this.schedulerService.CompleteMissionAsync(id);
            if (result is NotFoundOperationResult<Scheduler.Core.Models.Mission>)
            {
                return this.NotFound(id);
            }

            if (result is Scheduler.Core.Models.BadRequestOperationResult<Scheduler.Core.Models.Mission>)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = result.Description
                    });
            }

            await this.schedulerHubContext.Clients.All.MissionUpdated(id);
            var updatedMission = await this.missionProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(Mission), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<Mission>> ExecuteAsync(int id)
        {
            var result = await this.schedulerService.ExecuteMissionAsync(id);
            if (result is NotFoundOperationResult<Scheduler.Core.Models.Mission>)
            {
                return this.NotFound(id);
            }
            else if (result is Scheduler.Core.Models.BadRequestOperationResult<Scheduler.Core.Models.Mission>)
            {
                return this.BadRequest(result.Description);
            }

            await this.schedulerHubContext.Clients.All.MissionUpdated(id);
            var updatedMission = await this.missionProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(IEnumerable<Mission>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mission>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
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
                return this.BadRequest(e.Message);
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
                return this.BadRequest(e.Message);
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
                return this.NotFound(message);
            }

            return this.Ok(result);
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
                return this.BadRequest(e.Message);
            }
        }

        #endregion
    }
}
