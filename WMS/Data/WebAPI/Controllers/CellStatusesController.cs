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
    public class CellStatusesController :
        ControllerBase,
        IReadAllController<CellStatus>,
        IReadSingleController<CellStatus, int>
    {
        #region Fields

        private readonly ICellStatusProvider cellStatusProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CellStatusesController(
            ILogger<CellStatusesController> logger,
            ICellStatusProvider cellStatusProvider)
        {
            this.logger = logger;
            this.cellStatusProvider = cellStatusProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<CellStatus>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CellStatus>>> GetAllAsync()
        {
            return this.Ok(await this.cellStatusProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.cellStatusProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(CellStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CellStatus>> GetByIdAsync(int id)
        {
            var result = await this.cellStatusProvider.GetByIdAsync(id);
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
