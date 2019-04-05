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
using SchedulerBadRequestOperationResult =
    Ferretto.WMS.Scheduler.Core.Models.BadRequestOperationResult<System.Collections.Generic.IEnumerable<
        Ferretto.WMS.Scheduler.Core.Models.SchedulerRequest>>;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController :
        BaseController,
        ICreateController<ItemListDetails>,
        IReadAllPagedController<ItemList>,
        IReadSingleController<ItemListDetails, int>,
        IUpdateController<ItemListDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IItemListProvider itemListProvider;

        private readonly IItemListRowProvider itemListRowProvider;

        private readonly ILogger logger;

        private readonly ISchedulerService schedulerService;

        #endregion

        #region Constructors

        public ItemListsController(
            ILogger<ItemListsController> logger,
            IHubContext<SchedulerHub, ISchedulerHub> hubContext,
            IItemListProvider itemListProvider,
            IItemListRowProvider itemListRowProvider,
            ISchedulerService schedulerService)
            : base(hubContext)
        {
            this.logger = logger;
            this.itemListProvider = itemListProvider;
            this.itemListRowProvider = itemListRowProvider;
            this.schedulerService = schedulerService;
        }

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(ItemListDetails), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ItemListDetails>> CreateAsync(ItemListDetails model)
        {
            var result = await this.itemListProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemListDetails), result.Entity.Id, HubEntityOperation.Created);

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [HttpPost("{id}/execute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExecuteAsync(int id, int areaId, int? bayId = null)
        {
            var result = await this.schedulerService.ExecuteListAsync(id, areaId, bayId);

            if (result is SchedulerBadRequestOperationResult)
            {
                this.logger.LogWarning($"Request of execution for list (id={id}) could not be processed.");

                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = result.Description
                });
            }

            this.logger.LogInformation($"Request of execution for list (id={id}) was accepted.");
            await this.NotifyEntityUpdatedAsync(nameof(Scheduler.Core.Models.ItemList), id, HubEntityOperation.Updated);

            return this.Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ItemList>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                return this.Ok(
                    await this.itemListProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy.ParseSortOptions(),
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = e.Message
                });
            }
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(string where = null, string search = null)
        {
            try
            {
                return await this.itemListProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = e.Message
                });
            }
        }

        [ProducesResponseType(typeof(ItemListDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemListDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemListProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Detail = id.ToString(),
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<ItemListRow>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/rows")]
        public async Task<ActionResult<IEnumerable<ItemListRow>>> GetRowsAsync(int id)
        {
            var result = await this.itemListRowProvider.GetByItemListIdAsync(id);
            if (result == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Detail = id.ToString(),
                    Status = StatusCodes.Status404NotFound,
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
                return this.Ok(await this.itemListProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = e.Message
                });
            }
        }

        [HttpPost("{id}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ItemList>> SuspendAsync(int id)
        {
            var result = await this.schedulerService.SuspendListAsync(id);
            if (result is UnprocessableEntityOperationResult<ItemList>)
            {
                return this.UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = result.Description
                });
            }
            else if (result is NotFoundOperationResult<ItemList>)
            {
                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = result.Description
                });
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(ItemListDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch]
        public async Task<ActionResult<ItemListDetails>> UpdateAsync(ItemListDetails model)
        {
            var result = await this.itemListProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemListDetails>)
                {
                    return this.NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Description
                    });
                }

                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemListDetails), result.Entity.Id, HubEntityOperation.Updated);

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
