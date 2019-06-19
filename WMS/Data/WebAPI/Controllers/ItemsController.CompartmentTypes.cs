using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
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
                MaxCapacity = maxCapacity,
            });

            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.AddCompartmentTypeAssociationAsync), result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<ItemCompartmentType>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("item-compartment-types")]
        public async Task<ActionResult<IEnumerable<ItemCompartmentType>>> AddItemCompartmentTypesAsync(IEnumerable<ItemCompartmentType> models)
        {
            if (models == null)
            {
                return this.BadRequest();
            }

            var result = await this.itemCompartmentTypeProvider.CreateItemCompartmentTypesRangeByItemIdAsync(models);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.AddItemCompartmentTypesAsync), result.Entity);
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
                return this.NegativeResponse(result);
            }

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
                return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<ItemCompartmentType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpGet("{id}/unassociated-compartment-types")]
        public async Task<ActionResult<IEnumerable<ItemCompartmentType>>> GetAllUnassociatedCompartmentTypesByIdAsync(int id)
        {
            try
            {
                var result = await this.itemCompartmentTypeProvider.GetAllUnassociatedByItemIdAsync(id);
                return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
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
            var model = new ItemCompartmentType { ItemId = id, CompartmentTypeId = compartmentTypeId, MaxCapacity = maxCapacity };

            var result = await this.itemCompartmentTypeProvider.UpdateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
