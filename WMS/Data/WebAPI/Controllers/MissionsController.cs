using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        private readonly ISchedulerService schedulerService;

        #endregion

        #region Constructors

        public MissionsController(
            ILogger<MissionsController> logger,
            IMissionProvider missionProvider,
            ISchedulerService schedulerService)
        {
            this.logger = logger;
            this.missionProvider = missionProvider;
            this.schedulerService = schedulerService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404)]
        [HttpPost("{id}/abort")]
        public Task<ActionResult<Mission>> AbortAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<Scheduler.Core.Models.Mission>> CompleteAsync(int id)
        {
            var result = await this.schedulerService.CompleteMissionAsync(id);
            if (result is NotFoundOperationResult<Scheduler.Core.Models.Mission>)
            {
                return this.NotFound(id);
            }
            else if (result is Scheduler.Core.Models.BadRequestOperationResult<Scheduler.Core.Models.Mission>)
            {
                return this.BadRequest(result.Description);
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<Scheduler.Core.Models.Mission>> ExecuteAsync(int id)
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

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Mission>))]
        [ProducesResponseType(400)]
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

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404)]
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

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
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

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(400)]
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
