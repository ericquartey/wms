using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionOperationsController :
        BaseController,
        IReadAllPagedController<MissionOperation>,
        IReadSingleController<MissionOperation, int>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMissionOperationProvider missionOperationProvider;

        private readonly IMissionSchedulerService schedulerService;

        #endregion

        #region Constructors

        public MissionOperationsController(
            ILogger<MissionOperationsController> logger,
            IMissionOperationProvider missionOperationProvider,
            IMissionSchedulerService schedulerService)
        {
            this.logger = logger;
            this.missionOperationProvider = missionOperationProvider;
            this.schedulerService = schedulerService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(MissionOperation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/abort")]
        public async Task<ActionResult<MissionOperation>> AbortAsync(int id)
        {
            var result = await this.schedulerService.AbortOperationAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            var updatedMission = await this.missionOperationProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(MissionOperation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<MissionOperation>> CompleteItemAsync(int id, double quantity)
        {
            var result = await this.schedulerService.CompleteOperationAsync(id, quantity);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            var updatedOperation = await this.missionOperationProvider.GetByIdAsync(id);
            return this.Ok(updatedOperation);
        }

        [ProducesResponseType(typeof(MissionOperation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/execute")]
        public async Task<ActionResult<MissionOperation>> ExecuteAsync(int id)
        {
            var result = await this.schedulerService.ExecuteOperationAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            var updatedMission = await this.missionOperationProvider.GetByIdAsync(id);
            return this.Ok(updatedMission);
        }

        [ProducesResponseType(typeof(IEnumerable<MissionOperation>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MissionOperation>>> GetAllAsync(
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
                    await this.missionOperationProvider.GetAllAsync(
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
                return await this.missionOperationProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(MissionOperation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MissionOperation>> GetByIdAsync(int id)
        {
            var result = await this.missionOperationProvider.GetByIdAsync(id);
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

        #endregion
    }
}
