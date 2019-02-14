using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CellsController :
        ControllerBase,
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
            ICellProvider cellProvider,
            ILoadingUnitProvider loadingUnitProvider)
        {
            this.cellProvider = cellProvider;
            this.loadingUnitProvider = loadingUnitProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<Cell>))]
        [ProducesResponseType(400, Type = typeof(string))]
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
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<Cell>(where);

                return this.Ok(
                    await this.cellProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy,
                        whereExpression,
                        searchExpression));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<Cell>(where);

                return await this.cellProvider.GetAllCountAsync(
                           whereExpression,
                           searchExpression);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(CellDetails))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CellDetails>> GetByIdAsync(int id)
        {
            var result = await this.cellProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<LoadingUnitDetails>))]
        [HttpGet("{id}/loadingunits")]
        public async Task<ActionResult<IEnumerable<LoadingUnitDetails>>> GetLoadingUnitsAsync(int id)
        {
            return this.Ok(await this.loadingUnitProvider.GetByCellIdAsync(id));
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            return this.Ok(await this.cellProvider.GetUniqueValuesAsync(propertyName));
        }

        [ProducesResponseType(200, Type = typeof(CellDetails))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPatch]
        public async Task<ActionResult<CellDetails>> UpdateAsync(CellDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.cellProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<CellDetails>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        private static Expression<Func<Cell, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (c) =>
                c.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.AisleName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.AreaName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.LoadingUnitsDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Status.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Type.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Column.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Floor.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Number.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                c.Priority.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
