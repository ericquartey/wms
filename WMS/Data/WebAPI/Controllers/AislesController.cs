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
    public class AislesController :
        ControllerBase,
        IReadAllController<Aisle>,
        IReadSingleController<Aisle, int>
    {
        #region Fields

        private readonly IAisleProvider aisleProvider;

        private readonly ICellProvider cellProvider;

        private readonly ILogger<AislesController> logger;

        #endregion

        #region Constructors

        public AislesController(
            ILogger<AislesController> logger,
            IAisleProvider aisleProvider,
            ICellProvider cellProvider)
        {
            this.logger = logger;
            this.aisleProvider = aisleProvider;
            this.cellProvider = cellProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<Aisle>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aisle>>> GetAllAsync()
        {
            return this.Ok(await this.aisleProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.aisleProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(Aisle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Aisle>> GetByIdAsync(int id)
        {
            var result = await this.aisleProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<Cell>), StatusCodes.Status200OK)]
        [HttpGet("{id}/cells")]
        public async Task<ActionResult<IEnumerable<Cell>>> GetCellsAsync(int id)
        {
            return this.Ok(await this.cellProvider.GetByAisleIdAsync(id));
        }

        #endregion
    }
}
