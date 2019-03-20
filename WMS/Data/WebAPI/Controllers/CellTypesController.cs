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
    public class CellTypesController :
        ControllerBase,
        IReadAllController<CellType>,
        IReadSingleController<CellType, int>
    {
        #region Fields

        private readonly ICellTypeProvider cellTypeProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CellTypesController(
            ILogger<CellTypesController> logger,
            ICellTypeProvider cellTypeProvider)
        {
            this.logger = logger;
            this.cellTypeProvider = cellTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<CellType>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CellType>>> GetAllAsync()
        {
            return this.Ok(await this.cellTypeProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.cellTypeProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(CellType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CellType>> GetByIdAsync(int id)
        {
            var result = await this.cellTypeProvider.GetByIdAsync(id);
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
