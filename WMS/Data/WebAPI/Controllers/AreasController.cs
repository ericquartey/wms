using System.Collections.Generic;
using System.Linq;
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
    public class AreasController :
        ControllerBase,
        IReadAllController<Area>,
        IReadSingleController<Area, int>
    {
        #region Fields

        private readonly IAreaProvider areaProvider;

        private readonly IBayProvider bayProvider;

        private readonly ICellProvider cellProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public AreasController(
            ILogger<AreasController> logger,
            IAreaProvider areaProvider,
            IBayProvider bayProvider,
            ICellProvider cellProvider)
        {
            this.logger = logger;
            this.areaProvider = areaProvider;
            this.bayProvider = bayProvider;
            this.cellProvider = cellProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<Aisle>), StatusCodes.Status200OK)]
        [HttpGet("{id}/aisles")]
        public async Task<ActionResult<IEnumerable<Aisle>>> GetAisles(int id)
        {
            return this.Ok(await this.areaProvider.GetAislesAsync(id));
        }

        [ProducesResponseType(typeof(IEnumerable<Area>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Area>>> GetAllAsync()
        {
            return this.Ok(await this.areaProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.areaProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(IEnumerable<Bay>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/bays")]
        public async Task<ActionResult<IEnumerable<Bay>>> GetBaysAsync(int id)
        {
            var bays = await this.bayProvider.GetByAreaIdAsync(id);

            if (!bays.Any())
            {
                var message = $"No entity associated with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(bays);
        }

        [ProducesResponseType(typeof(Area), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Area>> GetByIdAsync(int id)
        {
            var result = await this.areaProvider.GetByIdAsync(id);
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

        [ProducesResponseType(typeof(IEnumerable<Cell>), StatusCodes.Status200OK)]
        [HttpGet("{id}/cells")]
        public async Task<ActionResult<IEnumerable<Cell>>> GetCellsAsync(int id)
        {
            return this.Ok(await this.cellProvider.GetByAreaIdAsync(id));
        }

        #endregion
    }
}
