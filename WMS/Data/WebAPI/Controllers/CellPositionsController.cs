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

        [ProducesResponseType(typeof(IEnumerable<CellPosition>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CellPosition>>> GetAllAsync()
        {
            return this.Ok(await this.cellPositionProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.cellPositionProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(CellPosition), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CellPosition>> GetByIdAsync(int id)
        {
            var result = await this.cellPositionProvider.GetByIdAsync(id);
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
