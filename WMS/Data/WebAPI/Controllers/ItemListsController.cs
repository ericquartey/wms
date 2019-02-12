using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController :
        ControllerBase,
        IReadAllPagedController<ItemList>,
        IReadSingleController<ItemList, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IItemListProvider itemListProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ItemListsController(
            ILogger<ItemListsController> logger,
            IItemListProvider itemListProvider)
        {
            this.logger = logger;
            this.itemListProvider = itemListProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemList>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
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

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet]
        [Route("api/[controller]/count")]
        public async Task<ActionResult<int>> GetAllCountAsync(string where = null, string search = null)
        {
            var searchExpression = BuildSearchExpression(search);
            var whereExpression = this.BuildWhereExpression<ItemList>(where);

            return await this.itemListProvider.GetAllCountAsync(
                       whereExpression,
                       searchExpression);
        }

        [ProducesResponseType(200, Type = typeof(ItemList))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemList>> GetByIdAsync(int id)
        {
            var result = await this.itemListProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet]
        [Route("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            return this.Ok(await this.itemListProvider.GetUniqueValuesAsync(propertyName));
        }

        private static Expression<Func<ItemList, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return i =>
                (i.Code != null && i.Code.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (i.Description != null && i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                i.ItemListItemsCount.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemListRowsCount.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
