using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController :
        ControllerBase,
        ICreateController<LoadingUnitCreating>,
        IReadAllPagedController<LoadingUnit>,
        IReadSingleController<LoadingUnitDetails, int>,
        IUpdateController<LoadingUnitDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            ILoadingUnitProvider loadingUnitProvider,
            ICompartmentProvider compartmentProvider)
        {
            this.loadingUnitProvider = loadingUnitProvider;
            this.compartmentProvider = compartmentProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(LoadingUnitCreating), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<LoadingUnitCreating>> CreateAsync(LoadingUnitCreating model)
        {
            var result = await this.loadingUnitProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<LoadingUnit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadingUnit>>> GetAllAsync(
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
                    await this.loadingUnitProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
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
                return await this.loadingUnitProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(typeof(LoadingUnitDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LoadingUnitDetails>> GetByIdAsync(int id)
        {
            var result = await this.loadingUnitProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<CompartmentDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<CompartmentDetails>>> GetCompartmentsAsync(int id)
        {
            var compartments = await this.compartmentProvider.GetByLoadingUnitIdAsync(id);

            return this.Ok(compartments);
        }

        [ProducesResponseType(typeof(LoadingUnitSize), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/size")]
        public async Task<ActionResult<LoadingUnitSize>> GetSizeByTypeIdAsync(int id)
        {
            var info = await this.loadingUnitProvider.GetSizeByTypeIdAsync(id);
            if (info != null)
            {
                return this.Ok(info);
            }
            else
            {
                return this.NotFound(info);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            try
            {
                return this.Ok(await this.loadingUnitProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(typeof(LoadingUnitDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch]
        public async Task<ActionResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.loadingUnitProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<LoadingUnitDetails>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
