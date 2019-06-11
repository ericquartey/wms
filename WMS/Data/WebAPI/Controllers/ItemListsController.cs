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

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController :
        BaseController,
        ICreateController<ItemListDetails>,
        IReadAllPagedController<ItemList>,
        IReadSingleController<ItemListDetails, int>,
        IUpdateController<ItemListDetails, int>,
        IDeleteController<int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IItemListProvider itemListProvider;

        private readonly IItemListRowProvider itemListRowProvider;

        private readonly ISchedulerService schedulerService;

        private readonly INotificationService notificationService;

        #endregion

        #region Constructors

        public ItemListsController(
            IHubContext<DataHub, IDataHub> hubContext,
            IItemListProvider itemListProvider,
            IItemListRowProvider itemListRowProvider,
            ISchedulerService schedulerService,
            INotificationService notificationService)
            : base(hubContext)
        {
            this.itemListProvider = itemListProvider;
            this.itemListRowProvider = itemListRowProvider;
            this.schedulerService = schedulerService;
            this.notificationService = notificationService;
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
                return this.NegativeResponse(result);
            }

            await this.notificationService.SendNotificationsAsync();

            return this.CreatedAtAction(nameof(this.CreateAsync), result.Entity);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await this.itemListProvider.DeleteAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.notificationService.SendNotificationsAsync();

            return this.Ok();
        }

        [HttpPost("{id}/execute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> ExecuteAsync(int id, int areaId, int? bayId = null)
        {
            var result = await this.schedulerService.ExecuteListAsync(id, areaId, bayId);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.notificationService.SendNotificationsAsync();

            return this.Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ItemList>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetAllAsync(
            int skip = 0,
            int take = 0,
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
                return await this.itemListProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
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
                return this.BadRequest(e);
            }
        }

        [HttpPost("{id}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ItemList>> SuspendAsync(int id)
        {
            var result = await this.schedulerService.SuspendListAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(ItemListDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<ItemListDetails>> UpdateAsync(ItemListDetails model, int id)
        {
            if (id != model?.Id)
            {
                return this.BadRequest();
            }

            var result = await this.itemListProvider.UpdateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.notificationService.SendNotificationsAsync();

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
