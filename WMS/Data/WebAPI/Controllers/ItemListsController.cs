using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.DataModels;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController : ControllerBase,
          IReadAllController<ItemList>,
          IReadSingleController<ItemList>
    {
        #region Fields

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
        public async Task<ActionResult<IEnumerable<ItemList>>> GetAllAsync()
        {
            try
            {
                return this.Ok(await this.itemListsProvider.GetAllAsync());
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entities.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
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

        #endregion
    }
}
