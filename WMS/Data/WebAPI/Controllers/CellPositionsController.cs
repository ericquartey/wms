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
    public class CellPositionsController :
        ControllerBase,
        IReadAllController<CellPosition>,
        IReadSingleController<CellPosition, int>
    {
        #region Fields

        private readonly ICellPositionProvider cellPositionProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CellPositionsController(
            ILogger<CellPositionsController> logger,
            ICellPositionProvider cellPositionProvider)
        {
            this.logger = logger;
            this.cellPositionProvider = cellPositionProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<CellPosition>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CellPosition>>> GetAllAsync()
        {
            return this.Ok(await this.cellPositionProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.cellPositionProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(CellPosition))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CellPosition>> GetByIdAsync(int id)
        {
            var result = await this.cellPositionProvider.GetByIdAsync(id);
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
