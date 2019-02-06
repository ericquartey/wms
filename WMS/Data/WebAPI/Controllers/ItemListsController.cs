using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController : ControllerBase,
        IReadAllPagedController<ItemList>,
        IReadSingleController<ItemList, int>
    {
        #region Fields

        private const string CollectionErrorMessage = "An error occurred while retrieving the requested set of entities.";

        private readonly IItemListsProvider itemListsProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ItemListsController(
            ILogger<ItemListsController> logger,
            IItemListsProvider itemListsProvider)
        {
            this.logger = logger;
            this.itemListsProvider = itemListsProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemList>))]
        [ProducesResponseType(500, Type = typeof(string))]
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
                    await this.itemListsProvider.GetAllAsync(
                        skip: skip,
                        take: take,
                        orderBy: orderBy,
                        whereExpression: whereExpression,
                        searchExpression: searchExpression));
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entities.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet]
        [Route("api/[controller]/count")]
        public async Task<ActionResult<int>> GetAllCountAsync(string where = null, string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<ItemList>(where);

                return await this.itemListsProvider.GetAllCountAsync(
                           whereExpression,
                           searchExpression);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, CollectionErrorMessage);
                return this.BadRequest(CollectionErrorMessage);
            }
        }

        [ProducesResponseType(200, Type = typeof(ItemList))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemList>> GetByIdAsync(int id)
        {
            try
            {
                var result = await this.itemListsProvider.GetByIdAsync(id);
                if (result == null)
                {
                    var message = $"No entity with the specified id={id} exists.";
                    this.logger.LogWarning(message);
                    return this.NotFound(message);
                }

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entity with id={id}.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
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
