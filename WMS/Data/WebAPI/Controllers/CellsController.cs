using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController :
        BaseController,
        IReadAllPagedController<Cell>,
        IReadSingleController<CellDetails, int>,
        IUpdateController<CellDetails, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ICellProvider cellProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        #endregion

        #region Constructors

        public CellsController(
            ICellProvider cellProvider,
            ILoadingUnitProvider loadingUnitProvider)
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
            int take = 0,
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
                return this.BadRequest(e);
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
                return this.BadRequest(e);
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
        [HttpGet("{id}/loading-units")]
        public async Task<ActionResult<IEnumerable<LoadingUnitDetails>>> GetLoadingUnitsAsync(int id)
        {
            return this.Ok(await this.loadingUnitProvider.GetAllByCellIdAsync(id));
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
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(CellDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<CellDetails>> UpdateAsync(CellDetails model, int id)
        {
            if (id != model?.Id)
            {
                return this.BadRequest();
            }

            var result = await this.cellProvider.UpdateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(CellDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}/operational-info")]
        public async Task<ActionResult<CellDetails>> UpdateOperationalInfoAsync(CellOperationalInfoUpdate model)
        {
            var result = await this.cellProvider.UpdateOperationalInfoAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
