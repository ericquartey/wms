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
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompartmentTypesController :
        BaseController,
        IReadAllPagedController<CompartmentType>,
        IReadSingleController<CompartmentType, int>
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CompartmentTypesController(
            ILogger<CompartmentTypesController> logger,
            IHubContext<DataHub, IDataHub> hubContext,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            ICompartmentTypeProvider compartmentTypeProvider)
            : base(hubContext)
        {
            this.logger = logger;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.compartmentTypeProvider = compartmentTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(CompartmentType), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<CompartmentType>> CreateAsync(
            CompartmentType model,
            int? itemId,
            int? maxCapacity)
        {
            var result = await this.compartmentTypeProvider.CreateAsync(model, itemId, maxCapacity);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(CompartmentType), result.Entity.Id, HubEntityOperation.Created);

            return this.CreatedAtAction(nameof(this.CreateAsync), result.Entity);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<ActionResult> DeleteItemAssociationAsync(int id, int itemId)
        {
            var result = await this.itemCompartmentTypeProvider.DeleteAsync(itemId, id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(Item), result.Entity.Id, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(CompartmentType), result.Entity.CompartmentTypeId, HubEntityOperation.Updated);

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<CompartmentType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompartmentType>>> GetAllAsync(
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
                    await this.compartmentTypeProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (System.NotSupportedException e)
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
                return await this.compartmentTypeProvider.GetAllCountAsync(where, search);
            }
            catch (System.NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<ItemCompartmentType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<ItemCompartmentType>>> GetAllItemAssociationsByIdAsync(int id)
        {
            try
            {
                var result = await this.itemCompartmentTypeProvider.GetAllByCompartmentTypeIdAsync(id);
                return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
            }
            catch (System.NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(CompartmentType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CompartmentType>> GetByIdAsync(int id)
        {
            var result = await this.compartmentTypeProvider.GetByIdAsync(id);
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

        #endregion
    }
}
