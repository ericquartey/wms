using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompartmentTypesController :
        BaseController,
        IReadAllPagedController<CompartmentType>,
        IReadSingleController<CompartmentType, int>,
        IDeleteController<int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        private readonly IItemProvider itemProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CompartmentTypesController(
            ILogger<CompartmentTypesController> logger,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            ICompartmentTypeProvider compartmentTypeProvider,
            IItemProvider itemProvider)
        {
            this.logger = logger;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.compartmentTypeProvider = compartmentTypeProvider;
            this.itemProvider = itemProvider;
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

            return this.CreatedAtAction(nameof(this.CreateAsync), result.Entity);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await this.compartmentTypeProvider.DeleteAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{compartmentTypeId}/items/{itemId}")]
        public async Task<ActionResult> DeleteItemAssociationAsync(int compartmentTypeId, int itemId)
        {
            var result = await this.itemCompartmentTypeProvider.DeleteAsync(itemId, compartmentTypeId);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<AssociateItemWithCompartmentType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/associated-items")]
        public async Task<ActionResult<IEnumerable<AssociateItemWithCompartmentType>>> GetAllAssociatedItemWithCompartmentTypeAsync(int id)
        {
            var result = await this.itemProvider.GetAllAssociatedByCompartmentTypeIdAsync(id);
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

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return this.Ok(await this.compartmentTypeProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        #endregion
    }
}
