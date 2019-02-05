using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController : ControllerBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly Models.IWarehouse warehouse;

        #endregion

        #region Constructors

        public ItemListsController(
            IServiceProvider serviceProvider,
            ILogger<ItemListsController> logger,
            Models.IWarehouse warehouse)
        {
            this.logger = logger;
            this.warehouse = warehouse;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<Models.ItemList>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet]
        public ActionResult<IEnumerable<Models.ItemList>> GetAll()
        {
            return this.Ok(this.warehouse.Lists);
        }

        [ProducesResponseType(200, Type = typeof(Models.ItemList))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public ActionResult<Models.ItemList> GetById(int id)
        {
            try
            {
                var result = this.warehouse.Lists.SingleOrDefault(l => l.Id == id);
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

        #endregion
    }
}
