using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ferretto.WMS.Data.WebAPI.Models.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        #region Fields

        private const string DEFAULT_ORDERBY_FIELD = nameof(Models.Item.Code);

        private static readonly System.Text.RegularExpressions.Regex binaryExpressionRegex =
               new System.Text.RegularExpressions.Regex(
           @"(?<operator>[^(]+)\((?<left>[^,]+),(?<right>[^)]+)\)",
           System.Text.RegularExpressions.RegexOptions.Compiled);

        private readonly ILogger logger;

        private readonly IServiceProvider serviceProvider;

        private readonly Models.IWarehouse warehouse;

        #endregion Fields

        #region Constructors

        public ItemsController(
            IServiceProvider serviceProvider,
            ILogger<ItemsController> logger,
            Models.IWarehouse warehouse)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.warehouse = warehouse;
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
                $"Get All items (skip:{skip}, take:{take}, orderBy:'{orderBy}', where:'{where}', search:'{search}')");

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

        [ProducesResponseType(200, Type = typeof(Models.Item))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpGet("{id}")]
        public ActionResult<Models.Item> GetById(int id)
        {
            try
            {
                var result = this.warehouse.Items.SingleOrDefault(i => i.Id == id);
                if (result == null)
                {
                    var message = string.Format("No entity with the specified id={0} exists.", id);
                    this.logger.LogWarning(message);
                    return this.NotFound(message);
                }

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                var message = string.Format("An error occurred while retrieving the requested entity with id={0}.", id);
                this.logger.LogError(ex, message);
                return this.BadRequest(message);
            }
        }

        private static IExpression BuildExpression(string where)
        {
            var match = binaryExpressionRegex.Match(where);
            if (match.Success)
            {
                var operatorName = match.Groups["operator"].Value;
                return new Models.Expressions.BinaryExpression(operatorName)
                {
                    LeftExpression = BuildExpression(match.Groups["left"].Value),
                    RightExpression = BuildExpression(match.Groups["right"].Value)
                };
            }
            else
            {
                return new ValueExpression(where);
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

            var expression = BuildExpression(where);

            var lambdaInParameter = Expression.Parameter(typeof(T), typeof(T).Name.ToLower());

            var lambdaBody = expression?.GetLambdaBody<Models.Item>(lambdaInParameter);

            return (Expression<Func<T, bool>>)Expression.Lambda(lambdaBody, lambdaInParameter);
        }

        private IQueryable<T> ApplyTransform<T>(int skip, int take, string orderBy, Expression<Func<T, bool>> where,
                    Expression<Func<T, bool>> searchFunction, IQueryable<T> entities)
        {
            // TODO: if skip or take, then orderby should be defined (exception)
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
