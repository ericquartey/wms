using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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
            IHubContext<DataHub, IDataHub> hubContext,
            ICompartmentProvider compartmentProvider)
            : base(hubContext)
        {
            this.compartmentProvider = compartmentProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(CompartmentDetails), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            var result = await this.compartmentProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(Compartment), model?.Id, HubEntityOperation.Created);

            return this.CreatedAtAction(nameof(this.CreateAsync), result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<CompartmentDetails>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("range")]
        public async Task<ActionResult<CompartmentDetails>> CreateRangeAsync(IEnumerable<CompartmentDetails> models)
        {
            var result = await this.compartmentProvider.CreateRangeAsync(models);

            if (models == null)
            {
                return this.BadRequest();
            }

            if (!result.Success)
            {
                return this.BadRequest(result);
            }

            foreach (var entity in result.Entity)
            {
                await this.NotifyEntityUpdatedAsync(nameof(Compartment), entity.Id, HubEntityOperation.Created);
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
                if (result is UnprocessableEntityOperationResult<CompartmentDetails>)
                {
                    return this.UnprocessableEntity(new ProblemDetails
                    {
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = result.Description
                    });
                }

                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(Compartment), id, HubEntityOperation.Deleted);

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
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(double?), StatusCodes.Status200OK)]
        [HttpGet("max-capacity")]
        public async Task<ActionResult<double?>> GetMaxCapacityAsync(double width, double height, int itemId)
        {
            return this.Ok(await this.compartmentProvider.GetMaxCapacityAsync(width, height, itemId));
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
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
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
                if (result is NotFoundOperationResult<CompartmentDetails>)
                {
                    return this.NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Description
                    });
                }

                return this.BadRequest(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(Compartment), result.Entity.Id, HubEntityOperation.Updated);

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
