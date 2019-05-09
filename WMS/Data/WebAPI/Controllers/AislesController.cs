using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AislesController :
        BaseController,
        IReadAllController<Aisle>,
        IReadSingleController<Aisle, int>
    {
        #region Fields

        private readonly IAisleProvider aisleProvider;

        private readonly ICellProvider cellProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly ILogger<AislesController> logger;

        #endregion

        #region Constructors

        public AislesController(
            ILogger<AislesController> logger,
            IHubContext<SchedulerHub, ISchedulerHub> hubContext,
            IAisleProvider aisleProvider,
            ILoadingUnitProvider loadingUnitProvider,
            ICellProvider cellProvider)
            : base(hubContext)
        {
            this.logger = logger;
            this.aisleProvider = aisleProvider;
            this.cellProvider = cellProvider;
            this.loadingUnitProvider = loadingUnitProvider;
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}/loadingunits")]
        public async Task<ActionResult<Aisle>> GetAllLoadingUnitsByIdAsync(
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

                var result = await this.loadingUnitProvider.GetAllByIdAisleAsync(
                    id,
                    skip,
                    take,
                    orderByExpression,
                    where,
                    search);

                if (result is NotFoundOperationResult<IEnumerable<LoadingUnit>>)
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
            catch (System.NotSupportedException e)
            {
                return this.BadRequest(e);
            }
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
            return this.Ok(await this.cellProvider.GetByAisleIdAsync(id));
        }

        #endregion
    }
}
