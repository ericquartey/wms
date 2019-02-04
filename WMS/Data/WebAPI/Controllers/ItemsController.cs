using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ItemsController : ControllerBase,
        IReadAllPagedController<Item>,
        IReadSingleController<Item>,
        IUpdateController<Item>
    {
        #region Fields

        private const string CollectionErrorMessage = "An error occurred while retrieving the requested set of entities.";

        private readonly IAreaProvider areaProvider;

        private readonly IItemProvider itemProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ItemsController(
            ILogger<ItemsController> logger,
            IItemProvider itemProvider,
            IAreaProvider areaProvider)
        {
            this.logger = logger;
            this.itemProvider = itemProvider;
            this.areaProvider = areaProvider;
        }

        #endregion

        #region Methods

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
            this.logger.LogInformation(
                $"Get Items (skip:{skip}, take:{take}, orderBy:'{orderBy}', where:'{where}', search:'{search}')");

            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<Item>(where);

                return this.Ok(
                    await this.itemProvider.GetAllAsync(
                        skip: skip,
                        take: take,
                        orderBy: orderBy,
                        whereExpression: whereExpression,
                        searchExpression: searchExpression));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, CollectionErrorMessage);
                return this.BadRequest(CollectionErrorMessage);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet]
        [Route("api/[controller]/count")]
        public async Task<ActionResult<int>> GetAllCount(
            string where = null,
            string search = null)
        {
            this.logger.LogInformation(
                $"Get Items count (where:'{where}', search:'{search}')");

            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<Item>(where);

                return await this.itemProvider.GetAllCountAsync(
                    whereExpression,
                    searchExpression);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, CollectionErrorMessage);
                return this.BadRequest(CollectionErrorMessage);
            }
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Area>))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}/areas_with_availability")]
        public async Task<ActionResult<IEnumerable<Area>>> GetAreasWithAvailability(int id)
        {
            try
            {
                var areas = await this.areaProvider.GetByItemIdAvailabilityAsync(id);
                if (!areas.Any())
                {
                    var message = $"No entity associated with the specified id={id} exists.";
                    this.logger.LogWarning(message);
                    return this.NotFound(message);
                }

                return this.Ok(areas);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entities associated with id={id}.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Item))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetByIdAsync(int id)
        {
            this.logger.LogInformation($"Get Item (id:{id})");

            try
            {
                var result = await this.itemProvider.GetByIdAsync(id);
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

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(400)]
        [HttpGet]
        [Route("api/[controller]/unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValues(
            string propertyName)
        {
            this.logger.LogInformation(
               $"Get unique values (propertyName:'{propertyName})'");

            try
            {
                return this.Ok(await this.itemProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, CollectionErrorMessage);
                return this.BadRequest(CollectionErrorMessage);
            }
        }

        [ProducesResponseType(200, Type = typeof(Item))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost]
        public async Task<ActionResult<Item>> UpdateAsync(Item model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            this.logger.LogInformation($"Update Item (id:{model.Id})");

            try
            {
                var updatedModel = await this.itemProvider.UpdateAsync(model);
                if (updatedModel == null)
                {
                    var message = $"No changes where applied to entity id={model.Id}.";
                    this.logger.LogWarning(message);
                    return this.Ok(message);
                }

                return this.Ok(updatedModel);
            }
            catch (ArgumentException ex)
            {
                var message = ex.Message;
                this.logger.LogError(ex, message);
                return this.NotFound(message);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entity with id={model.Id}.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        private static Expression<Func<Item, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                (i.AbcClassDescription != null && i.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (i.Description != null && i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (i.ItemCategoryDescription != null && i.ItemCategoryDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                i.TotalAvailable.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase) == true;
        }

        #endregion
    }
}
