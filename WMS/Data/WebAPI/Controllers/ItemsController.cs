using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult GetAll(int skip = 0, int take = int.MaxValue, string orderBy = DEFAULT_ORDERBY_FIELD)
        {
            try
            {
                var orderByField = string.IsNullOrWhiteSpace(orderBy) ? DEFAULT_ORDERBY_FIELD : orderBy;
                var skipValue = skip < 0 ? 0 : skip;
                var takeValue = take < 0 ? int.MaxValue : take;

                // var expression = this.CreateSelectorExpression<Common.DataModels.Item, object>(orderByField);

                var result = this.warehouse.Items
                    .AsQueryable()
                    .Skip(skipValue)
                    .Take(takeValue)
                    //   .OrderBy(expression)
                    .Select(i => new Models.Item
                    {
                        Id = i.Id,
                        Code = i.Code
                    }
                    );

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while retrieving the requested set of entities.");
                return this.BadRequest(ex.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Models.Item))]
        [ProducesResponseType(400, Type = typeof(SerializableError))]
        [ProducesResponseType(404, Type = typeof(SerializableError))]
        [HttpGet("{id}")]
        public ActionResult GetById(int id)
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

        #endregion Methods
    }
}
