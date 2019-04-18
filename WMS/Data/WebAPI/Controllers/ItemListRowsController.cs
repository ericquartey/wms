using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Ferretto.WMS.Scheduler.Core.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListRowsController :
        BaseController,
        ICreateController<ItemListRowDetails>,
        IReadAllPagedController<ItemListRow>,
        IReadSingleController<ItemListRowDetails, int>,
        IUpdateController<ItemListRowDetails, int>,
        IGetUniqueValuesController,
        IDeleteController<int>
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider;

        private readonly ILogger logger;

        private readonly ISchedulerService schedulerService;

        #endregion

        #region Constructors

        public ItemListRowsController(
            ILogger<ItemListRowsController> logger,
            IHubContext<SchedulerHub, ISchedulerHub> hubContext,
            ISchedulerService schedulerService,
            IItemListRowProvider itemListRowProvider)
            : base(hubContext)
        {
            this.logger = logger;
            this.schedulerService = schedulerService;
            this.itemListRowProvider = itemListRowProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(ItemListRowDetails), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            var result = await this.itemListRowProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemListRow), result.Entity.Id, HubEntityOperation.Created);
            await this.NotifyEntityUpdatedAsync(nameof(ItemList), result.Entity.ItemListId, HubEntityOperation.Updated);

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await this.itemListRowProvider.DeleteAsync(id);

            if (!result.Success)
            {
                if (result is UnprocessableEntityOperationResult<ItemListRowDetails>)
                {
                    return this.UnprocessableEntity(new ProblemDetails
                    {
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = result.Description
                    });
                }

                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemListRow), id, HubEntityOperation.Deleted);
            await this.NotifyEntityUpdatedAsync(nameof(ItemList), result.Entity.ItemListId, HubEntityOperation.Updated);

            return this.Ok();
        }

        [HttpPost("{id}/execute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ExecuteAsync(int id, int areaId, int? bayId = null)
        {
            var result = await this.schedulerService.ExecuteListRowAsync(id, areaId, bayId);
            if (result is UnprocessableEntityOperationResult<ItemListRow>)
            {
                this.logger.LogWarning($"Request of execution for list row (id={id}) could not be processed.");

                return this.UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemListRow), result.Entity.ListRowId, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(Mission), -1, HubEntityOperation.Created);
            await this.NotifyEntityUpdatedAsync(nameof(ItemList), id, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(SchedulerRequest), result.Entity.Id, HubEntityOperation.Created);

            this.logger.LogInformation($"Request of execution for list row (id={id}) was accepted.");

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<ItemListRow>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemListRow>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                return this.Ok(
                    await this.itemListRowProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy.ParseSortOptions(),
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
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(string where = null, string search = null)
        {
            try
            {
                return await this.itemListRowProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(ItemListRowDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemListRowDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemListRowProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = message
                });
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
                return this.Ok(await this.itemListRowProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        [HttpPost("{id}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> SuspendAsync(int id)
        {
            var result = await this.schedulerService.SuspendListRowAsync(id);
            if (result is UnprocessableEntityOperationResult<ItemListRow>)
            {
                return this.UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = result.Description
                });
            }
            else if (result is NotFoundOperationResult<ItemListRow>)
            {
                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = result.Description
                });
            }

            this.logger.LogInformation($"Request of execution for list row (id={id}) was accepted.");

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model, int id)
        {
            if (id != model?.Id)
            {
                return this.BadRequest();
            }

            var result = await this.itemListRowProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemListRowDetails>)
                {
                    return this.NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Description
                    });
                }

                return this.BadRequest(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemListRow), result.Entity.Id, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(ItemList), result.Entity.ItemListId, HubEntityOperation.Updated);

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
