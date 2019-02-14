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
    public class ItemListsController :
        ControllerBase,
        ICreateController<ItemListDetails>,
        IReadAllPagedController<ItemList>,
        IReadSingleController<ItemListDetails, int>,
        IUpdateController<ItemListDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IItemListProvider itemListProvider;

        private readonly IItemListRowProvider itemListRowProvider;

        #endregion

        #region Constructors

        public ItemListsController(
            IItemListProvider itemListProvider,
            IItemListRowProvider itemListRowProvider)
        {
            this.itemListProvider = itemListProvider;
            this.itemListRowProvider = itemListRowProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(201, Type = typeof(ItemListDetails))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<ItemListDetails>> CreateAsync(ItemListDetails model)
        {
            var result = await this.itemListProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemList>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<ItemList>(where);

                return this.Ok(
                    await this.itemListProvider.GetAllAsync(
                        skip: skip,
                        take: take,
                        orderBy: orderBy,
                        whereExpression: whereExpression,
                        searchExpression: searchExpression));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(string where = null, string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<ItemList>(where);

                return await this.itemListProvider.GetAllCountAsync(
                           whereExpression,
                           searchExpression);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(ItemListDetails))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemListDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemListRowProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(ItemListRow))]
        [ProducesResponseType(404)]
        [HttpGet("{id}/rows")]
        public async Task<ActionResult<ItemListRow>> GetRowsAsync(int id)
        {
            var result = await this.itemListRowProvider.GetByItemListIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            return this.Ok(await this.itemListProvider.GetUniqueValuesAsync(propertyName));
        }

        [ProducesResponseType(200, Type = typeof(ItemListDetails))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPatch]
        public async Task<ActionResult<ItemListDetails>> UpdateAsync(ItemListDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.itemListProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemListDetails>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        private static Expression<Func<ItemList, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return i =>
                i.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemListItemsCount.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemListRowsCount.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
