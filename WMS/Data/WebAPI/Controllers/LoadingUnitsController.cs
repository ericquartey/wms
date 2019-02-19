using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController :
        ControllerBase,
        ICreateController<LoadingUnitDetails>,
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

        [ProducesResponseType(201, Type = typeof(LoadingUnitDetails))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<LoadingUnitDetails>> CreateAsync(LoadingUnitDetails model)
        {
            var result = await this.loadingUnitProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<LoadingUnit>))]
        [ProducesResponseType(400, Type = typeof(string))]
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
                var whereExpression = where.AsIExpression();
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(
                    await this.loadingUnitProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        whereExpression,
                        search));
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
                var whereExpression = where.AsIExpression();

                return await this.loadingUnitProvider.GetAllCountAsync(
                           whereExpression,
                           search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(LoadingUnitDetails))]
        [ProducesResponseType(404)]
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

        [ProducesResponseType(200, Type = typeof(IEnumerable<CompartmentDetails>))]
        [ProducesResponseType(404)]
        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<CompartmentDetails>>> GetCompartmentsAsync(int id)
        {
            var compartments = await this.compartmentProvider.GetByLoadingUnitIdAsync(id);

            return this.Ok(compartments);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(400)]
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

        [ProducesResponseType(200, Type = typeof(LoadingUnitDetails))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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
