using System;
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

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController :
        BaseController,
        IReadAllPagedController<Cell>,
        IReadSingleController<CellDetails, int>,
        IUpdateController<CellDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ICellProvider cellProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        #endregion

        #region Constructors

        public CellsController(
            IHubContext<SchedulerHub, ISchedulerHub> hubContext,
            ICellProvider cellProvider,
            ILoadingUnitProvider loadingUnitProvider)
            : base(hubContext)
        {
            this.cellProvider = cellProvider;
            this.loadingUnitProvider = loadingUnitProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<Cell>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cell>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(
                    await this.cellProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = e.Message
                });
            }
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                return await this.cellProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = e.Message
                });
            }
        }

        [ProducesResponseType(typeof(CellDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CellDetails>> GetByIdAsync(int id)
        {
            var result = await this.cellProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Detail = id.ToString(),
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<LoadingUnitDetails>), StatusCodes.Status200OK)]
        [HttpGet("{id}/loadingunits")]
        public async Task<ActionResult<IEnumerable<LoadingUnitDetails>>> GetLoadingUnitsAsync(int id)
        {
            return this.Ok(await this.loadingUnitProvider.GetByCellIdAsync(id));
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return this.Ok(await this.cellProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = e.Message
                });
            }
        }

        [ProducesResponseType(typeof(CellDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch]
        public async Task<ActionResult<CellDetails>> UpdateAsync(CellDetails model)
        {
            var result = await this.cellProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<CellDetails>)
                {
                    return this.NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Description
                    });
                }

                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(CellDetails), result.Entity.Id, HubEntityOperation.Updated);

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
