using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreasController :
        BaseController,
        IReadAllController<Area>,
        IReadSingleController<Area, int>
    {
        #region Fields

        private readonly IAreaProvider areaProvider;

        private readonly IBayProvider bayProvider;

        private readonly ICellProvider cellProvider;

        private readonly IItemListProvider itemListProvider;

        private readonly IItemProvider itemProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public AreasController(
                            ILogger<AreasController> logger,
            IHubContext<DataHub, IDataHub> hubContext,
            IAreaProvider areaProvider,
            IBayProvider bayProvider,
            ICellProvider cellProvider,
            IItemProvider itemProvider,
            IItemListProvider itemListProvider)
            : base(hubContext)
        {
            this.logger = logger;
            this.areaProvider = areaProvider;
            this.bayProvider = bayProvider;
            this.cellProvider = cellProvider;
            this.itemProvider = itemProvider;
            this.itemListProvider = itemListProvider;
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

        [ProducesResponseType(typeof(IEnumerable<ItemList>), StatusCodes.Status200OK)]
        [HttpGet("{id}/itemlists")]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetItemListsAsync(int id)
        {
            return this.Ok(await this.itemListProvider.GetByAreaIdAsync(id));
        }

        [ProducesResponseType(typeof(IEnumerable<Item>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsAsync(
                    int id,
                    int skip = 0,
                    int take = int.MaxValue,
                    string where = null,
                    string orderBy = null,
                    string search = null)
        {
            try
            {
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(await this.itemProvider.GetByAreaIdAsync(
                        id,
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        #endregion
    }
}
