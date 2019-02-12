using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController :
        ControllerBase,
        ICreateController<ItemDetails>,
        IReadAllPagedController<Item>,
        IReadSingleController<ItemDetails, int>,
        IUpdateController<ItemDetails>
    {
        #region Fields

        private readonly IAreaProvider areaProvider;

        private readonly IItemProvider itemProvider;

        #endregion

        #region Constructors

        public ItemsController(
            IItemProvider itemProvider,
            IAreaProvider areaProvider)
        {
            this.itemProvider = itemProvider;
            this.areaProvider = areaProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(201, Type = typeof(IEnumerable<ItemDetails>))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            var result = await this.itemProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Item>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            var searchExpression = BuildSearchExpression(search);
            var whereExpression = this.BuildWhereExpression<Item>(where);

            return this.Ok(
                await this.itemProvider.GetAllAsync(
                    skip,
                    take,
                    orderBy,
                    whereExpression,
                    searchExpression));
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet]
        [Route("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            var searchExpression = BuildSearchExpression(search);
            var whereExpression = this.BuildWhereExpression<Item>(where);

            return await this.itemProvider.GetAllCountAsync(
                       whereExpression,
                       searchExpression);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Area>))]
        [ProducesResponseType(404)]
        [HttpGet("{id}/areas_with_availability")]
        public async Task<ActionResult<IEnumerable<Area>>> GetAreasWithAvailabilityAsync(int id)
        {
            var areas = await this.areaProvider.GetByItemIdAvailabilityAsync(id);

            return this.Ok(areas);
        }

        [ProducesResponseType(200, Type = typeof(ItemDetails))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet]
        [Route("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            return this.Ok(await this.itemProvider.GetUniqueValuesAsync(propertyName));
        }

        [ProducesResponseType(200, Type = typeof(ItemDetails))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpPatch]
        public async Task<ActionResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.itemProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemDetails>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(201, Type = typeof(Scheduler.Core.SchedulerRequest))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [HttpPost(nameof(Withdraw))]
        public async Task<IActionResult> Withdraw([FromBody] Scheduler.Core.SchedulerRequest request)
        {
            if (request == null)
            {
                return this.BadRequest();
            }

            var acceptedRequest = await this.warehouse.WithdrawAsync(request);
            if (acceptedRequest == null)
            {
                return this.UnprocessableEntity(this.ModelState);
            }

            return this.CreatedAtAction(nameof(this.Withdraw), new { id = acceptedRequest.Id }, acceptedRequest);
        }

        private static Expression<Func<Item, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                (i.AbcClassDescription != null &&
                 i.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (i.Description != null && i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (i.ItemCategoryDescription != null &&
                 i.ItemCategoryDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                i.TotalAvailable.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
