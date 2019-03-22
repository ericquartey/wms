using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
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

        [ProducesResponseType(typeof(IEnumerable<ItemCategory>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCategory>>> GetAllAsync()
        {
            return this.Ok(await this.itemCategoryProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.itemCategoryProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(ItemCategory), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemCategory>> GetByIdAsync(int id)
        {
            var result = await this.itemCategoryProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(result);
        }

        #endregion
    }
}
