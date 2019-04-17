using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    public partial class ItemsController
    {
        #region Fields

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Methods

        [ProducesResponseType(typeof(ItemCompartmentType), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{id}/compartment-types")]
        public async Task<ActionResult<ItemCompartmentType>> AddCompartmentTypeAssociationAsync(int id, int compartmentTypeId, int? maxCapacity)
        {
            var result = await this.itemCompartmentTypeProvider.CreateAsync(new ItemCompartmentType
            {
                CompartmentTypeId = compartmentTypeId,
                ItemId = id,
                MaxCapacity = maxCapacity
            });

            if (!result.Success)
            {
                return this.BadRequest(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(Item), result.Entity.ItemId, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(CompartmentType), result.Entity.CompartmentTypeId, HubEntityOperation.Updated);

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [HttpDelete("{id}/compartment-types/{compartmentTypeId}")]
        public async Task<ActionResult> DeleteCompartmentTypeAssociationAsync(int id, int compartmentTypeId)
        {
            var result = await this.itemCompartmentTypeProvider.DeleteAsync(id, compartmentTypeId);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemDetails>)
                {
                    return this.NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Description
                    });
                }

                return this.UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(Item), result.Entity.Id, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(CompartmentType), result.Entity.CompartmentTypeId, HubEntityOperation.Updated);

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<ItemCompartmentType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id}/compartment-types")]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllCompartmentTypeAssociationsByIdAsync(int id)
        {
            try
            {
                return this.Ok(
                    await this.itemCompartmentTypeProvider.GetAllByItemIdAsync(id));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [HttpPatch("{id}/compartment-types/{compartmentTypeId}")]
        public async Task<ActionResult> UpdateCompartmentTypeAssociationAsync(int id, int compartmentTypeId, double? maxCapacity)
        {
            var result = await this.itemCompartmentTypeProvider.UpdateAsync(
                new ItemCompartmentType { ItemId = id, CompartmentTypeId = compartmentTypeId, MaxCapacity = maxCapacity });

            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemDetails>)
                {
                    return this.NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Description
                    });
                }

                return this.UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(Item), result.Entity.Id, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(CompartmentType), result.Entity.CompartmentTypeId, HubEntityOperation.Updated);

            return this.Ok();
        }

        #endregion
    }
}
