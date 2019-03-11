using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemCategoriesController :
        ControllerBase,
        IReadAllController<ItemCategory>,
        IReadSingleController<ItemCategory, int>
    {
        #region Fields

        private readonly IItemCategoryProvider itemCategoryProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ItemCategoriesController(
            ILogger<ItemCategoriesController> logger,
            IItemCategoryProvider itemCategoryProvider)
        {
            this.logger = logger;
            this.itemCategoryProvider = itemCategoryProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemCategory>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCategory>>> GetAllAsync()
        {
            return this.Ok(await this.itemCategoryProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.itemCategoryProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(ItemCategory))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemCategory>> GetByIdAsync(int id)
        {
            var result = await this.itemCategoryProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        #endregion
    }
}
