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
    public class CompartmentsController :
        ControllerBase,
        ICreateController<CompartmentDetails>,
        IReadAllPagedController<Compartment>,
        IReadSingleController<CompartmentDetails, int>,
        IUpdateController<CompartmentDetails>,
        IDeleteController<CompartmentDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider;

        #endregion

        #region Constructors

        public CompartmentsController(
            ICompartmentProvider compartmentProvider)
        {
            this.compartmentProvider = compartmentProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(201, Type = typeof(CompartmentDetails))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            var result = await this.compartmentProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(201, Type = typeof(IEnumerable<CompartmentDetails>))]
        [ProducesResponseType(400)]
        [HttpPost("range")]
        public async Task<ActionResult<CompartmentDetails>> CreateRangeAsync(IEnumerable<CompartmentDetails> models)
        {
            var result = await this.compartmentProvider.CreateRangeAsync(models);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpDelete]
        public async Task<ActionResult> DeleteAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.compartmentProvider.DeleteAsync(model.Id);

            if (!result.Success)
            {
                return this.NotFound();
            }

            return this.Ok();
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Compartment>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compartment>>> GetAllAsync(
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
                    await this.compartmentProvider.GetAllAsync(
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

                return await this.compartmentProvider.GetAllCountAsync(
                           whereExpression,
                           search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<AllowedItemInCompartment>))]
        [HttpGet("{id}/allowed_items")]
        public async Task<ActionResult<IEnumerable<AllowedItemInCompartment>>> GetAllowedItemsAsync(int id)
        {
            return this.Ok(await this.compartmentProvider.GetAllowedItemsAsync(id));
        }

        [ProducesResponseType(200, Type = typeof(CompartmentDetails))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CompartmentDetails>> GetByIdAsync(int id)
        {
            var result = await this.compartmentProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(int?))]
        [ProducesResponseType(400)]
        [HttpGet("max_capacity")]
        public async Task<ActionResult<int?>> GetMaxCapacityAsync(int width, int height, int itemId)
        {
            return this.Ok(await this.compartmentProvider.GetMaxCapacityAsync(width, height, itemId));
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(400)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return this.Ok(await this.compartmentProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(CompartmentDetails))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPatch]
        public async Task<ActionResult<CompartmentDetails>> UpdateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.compartmentProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<CompartmentDetails>)
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
