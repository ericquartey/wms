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
    public class CompartmentsController :
        BaseController,
        ICreateController<CompartmentDetails>,
        IReadAllPagedController<Compartment>,
        IReadSingleController<CompartmentDetails, int>,
        IUpdateController<CompartmentDetails, int>,
        IDeleteController<int>,
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

        [ProducesResponseType(typeof(CompartmentDetails), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost]
        public async Task<ActionResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            var result = await this.compartmentProvider.CreateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.CreateAsync), result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<CompartmentDetails>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("range")]
        public async Task<ActionResult<CompartmentDetails>> CreateRangeAsync(IEnumerable<CompartmentDetails> models)
        {
            if (models == null)
            {
                return this.BadRequest();
            }

            var result = await this.compartmentProvider.CreateRangeAsync(models);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.CreateRangeAsync), result.Entity);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await this.compartmentProvider.DeleteAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<Compartment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compartment>>> GetAllAsync(
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
                    await this.compartmentProvider.GetAllAsync(
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
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                return await this.compartmentProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<AllowedItemInCompartment>), StatusCodes.Status200OK)]
        [HttpGet("{id}/allowed-items")]
        public async Task<ActionResult<IEnumerable<AllowedItemInCompartment>>> GetAllowedItemsAsync(int id)
        {
            // TODO: return 404 if a compartment with the specified id does not exist
            return this.Ok(await this.compartmentProvider.GetAllowedItemsAsync(id));
        }

        [ProducesResponseType(typeof(CompartmentDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CompartmentDetails>> GetByIdAsync(int id)
        {
            var result = await this.compartmentProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(double?), StatusCodes.Status200OK)]
        [HttpGet("max-capacity")]
        public async Task<ActionResult<double?>> GetMaxCapacityAsync(double width, double depth, int itemId)
        {
            return this.Ok(await this.compartmentProvider.GetMaxCapacityAsync(width, depth, itemId));
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return this.Ok(await this.compartmentProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(CompartmentDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<CompartmentDetails>> UpdateAsync(CompartmentDetails model, int id)
        {
            if (id != model?.Id)
            {
                return this.BadRequest();
            }

            var result = await this.compartmentProvider.UpdateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
