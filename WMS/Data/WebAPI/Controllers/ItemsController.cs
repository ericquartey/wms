using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase,
        IReadAllPagedController<Models.Item>,
        IReadSingleController<Models.Item>,
        IUpdateController<Models.Item>
    {
        #region Fields

        private const string CollectionErrorMessage = "An error occurred while retrieving the requested set of entities.";

        private readonly ILogger logger;

        private readonly Models.IWarehouse warehouse;

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public ItemsController(
            ILogger<ItemsController> logger,
            Models.IWarehouse warehouse,
            DatabaseContext context)
        {
            this.logger = logger;
            this.warehouse = warehouse;
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<Models.Item>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpGet]
        public ActionResult<IEnumerable<Models.Item>> GetAll(
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

                var whereExpression = BuildWhereExpression<Models.Item>(where);

                var transformedItems = this.ApplyTransform(
                    skip: skip,
                    take: take,
                    orderBy: orderBy,
                    where: whereExpression,
                    searchFunction: searchExpression,
                    entities: this.warehouse.Items.AsQueryable());

                return transformedItems.ToArray();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, CollectionErrorMessage);
                return this.BadRequest(CollectionErrorMessage);
            }
        }

        [ProducesResponseType(200, Type = typeof(Models.Item))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpGet("{id}")]
        public ActionResult<Models.Item> GetById(int id)
        {
            this.logger.LogInformation($"Get Item (id:{id})");

            try
            {
                var result = this.warehouse.Items.SingleOrDefault(i => i.Id == id);
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
                return this.BadRequest(message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Models.Item))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpPost]
#pragma warning disable S3242 // Method parameters should be declared with base types
        public async Task<ActionResult> UpdateAsync(Models.Item model)
#pragma warning restore S3242 // Method parameters should be declared with base types
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            this.logger.LogInformation($"Update Item (id:{model.Id})");

            try
            {
                var existingItem = this.warehouse.Items.SingleOrDefault(i => i.Id == model.Id);
                if (existingItem == null)
                {
                    var message = $"No entity with the specified id={model.Id} exists.";
                    this.logger.LogWarning(message);
                    return this.NotFound(message);
                }

                var updatedItem = await this.warehouse.UpdateAsync(existingItem);

                return this.Ok(updatedItem);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entity with id={model.Id}.";
                this.logger.LogError(ex, message);
                return this.BadRequest(message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Models.Area))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("{id}/areas_with_availability")]
        public ActionResult<IEnumerable<Models.Area>> GetAreasWithAvailability(int id)
        {
            try
            {
                var areas = this.dataContext.Compartments
                                .Include(c => c.LoadingUnit)
                                .ThenInclude(l => l.Cell)
                                .ThenInclude(c => c.Aisle)
                                .ThenInclude(a => a.Area)
                                .Where(c => c.ItemId == id)
                                .Where(c => (c.Stock - c.ReservedForPick + c.ReservedToStore) > 0)
                                .Select(c => new
                                {
                                    Id = c.LoadingUnit.Cell.Aisle.AreaId,
                                    Name = c.LoadingUnit.Cell.Aisle.Area.Name,
                                })
                                .Distinct()
                                .Select(c => new Models.Area
                                {
                                    Id = c.Id,
                                    Name = c.Name,
                                });

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
                return this.BadRequest(message);
            }
        }

        private static Expression<Func<Models.Item, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                i.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemCategoryDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.TotalAvailable.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private static Expression<Func<T, bool>> BuildWhereExpression<T>(string where)
        {
            if (string.IsNullOrWhiteSpace(where))
            {
                return null;
            }

            var lambdaInParameter = Expression.Parameter(typeof(T), typeof(T).Name.ToLower());

            var expression = where.ParseExpression();

            var lambdaBody = expression?.GetLambdaBody<T>(lambdaInParameter);

            return (Expression<Func<T, bool>>)Expression.Lambda(lambdaBody, lambdaInParameter);
        }

        private IQueryable<T> ApplyTransform<T>(
            int skip,
            int take,
            string orderBy,
            Expression<Func<T, bool>> where,
            Expression<Func<T, bool>> searchFunction,
            IQueryable<T> entities)
        {
            // TODO: if skip or take, then orderby should be defined (throw exception)
            var filteredItems = entities;
            if (where != null)
            {
                filteredItems = filteredItems.Where(where);
            }

            if (searchFunction != null)
            {
                filteredItems = filteredItems.Where(searchFunction);
            }

            filteredItems = this.ApplyOrderByClause(orderBy, filteredItems);

            var skipValue = skip < 0 ? 0 : skip;
            if (skipValue > 0)
            {
                filteredItems = filteredItems.Skip(skipValue);
            }

            var takeValue = take < 0 ? int.MaxValue : take;
            if (takeValue != int.MaxValue)
            {
                filteredItems = filteredItems.Take(takeValue);
            }

            return filteredItems;
        }

        #endregion Methods
    }
}
