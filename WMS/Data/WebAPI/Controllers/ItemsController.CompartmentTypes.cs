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

            return this.CreatedAtAction(nameof(this.AddCompartmentTypeAssociationAsync), result.Entity);
        }

        [ProducesResponseType(typeof(ItemCompartmentType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{id}/compartment-types/{compartmentTypeId}")]
        public async Task<ActionResult<ItemCompartmentType>> DeleteCompartmentTypeAssociationAsync(int id, int compartmentTypeId)
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

            await this.NotifyEntityUpdatedAsync(nameof(Item), id, HubEntityOperation.Updated);
            await this.NotifyEntityUpdatedAsync(nameof(CompartmentType), compartmentTypeId, HubEntityOperation.Updated);

            return this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<ItemCompartmentType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpGet("{id}/compartment-types")]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllCompartmentTypeAssociationsByIdAsync(int id)
        {
            try
            {
                var result = await this.itemCompartmentTypeProvider.GetAllByItemIdAsync(id);

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

                return this.Ok(result.Entity);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(ItemCompartmentType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPatch("{id}/compartment-types/{compartmentTypeId}")]
        public async Task<ActionResult<ItemCompartmentType>> UpdateCompartmentTypeAssociationAsync(
            int id,
            int compartmentTypeId,
            double? maxCapacity)
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

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
