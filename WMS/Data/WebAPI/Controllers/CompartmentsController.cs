using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Compartment>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            var searchExpression = BuildSearchExpression(search);
            var whereExpression = this.BuildWhereExpression<Compartment>(where);

            return this.Ok(
                await this.compartmentProvider.GetAllAsync(
                    skip,
                    take,
                    orderBy,
                    whereExpression,
                    searchExpression));
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            var searchExpression = BuildSearchExpression(search);
            var whereExpression = this.BuildWhereExpression<Compartment>(where);

            return await this.compartmentProvider.GetAllCountAsync(
                       whereExpression,
                       searchExpression);
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
        public async Task<ActionResult<int?>> GetMaxCapacity(int width, int height, int itemId)
        {
            return this.Ok(await this.compartmentProvider.GetMaxCapacityAsync(width, height, itemId));
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            return this.Ok(await this.compartmentProvider.GetUniqueValuesAsync(propertyName));
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

        private static Expression<Func<Compartment, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (c) =>
                (c.CompartmentStatusDescription != null &&
                 c.CompartmentStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.ItemDescription != null &&
                 c.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.ItemMeasureUnit != null &&
                 c.ItemMeasureUnit.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.LoadingUnitCode != null &&
                 c.LoadingUnitCode.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.Lot != null &&
                 c.Lot.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.MaterialStatusDescription != null &&
                 c.MaterialStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.Sub1 != null &&
                 c.Sub1.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                (c.Sub2 != null &&
                 c.Sub2.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                ||
                c.Stock.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
