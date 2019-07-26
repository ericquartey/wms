using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Resources;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Compartment = Ferretto.WMS.Data.Core.Models.Compartment;
using Item = Ferretto.WMS.Data.Core.Models.Item;
using ItemArea = Ferretto.WMS.Data.Core.Models.ItemArea;
using LoadingUnit = Ferretto.WMS.Data.Core.Models.LoadingUnit;
using SchedulerRequest = Ferretto.WMS.Data.Core.Models.ItemSchedulerRequest;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ItemsController :
        BaseController,
        ICreateController<ItemDetails>,
        IReadAllPagedController<Item>,
        IReadSingleController<ItemDetails, int>,
        IUpdateController<ItemDetails, int>,
        IGetUniqueValuesController,
        IDeleteController<int>
    {
        #region Fields

        private readonly IAreaProvider areaProvider;

        private readonly ICompartmentProvider compartmentProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly IItemAreaProvider itemAreaProvider;

        private readonly IItemProvider itemProvider;

        private readonly IItemSchedulerService schedulerService;

        #endregion

        #region Constructors

        public ItemsController(
            IItemProvider itemProvider,
            IAreaProvider areaProvider,
            IItemAreaProvider itemAreaProvider,
            ICompartmentProvider compartmentProvider,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider,
            ILoadingUnitProvider loadingUnitProvider,
            IItemSchedulerService schedulerService)
        {
            this.itemProvider = itemProvider;
            this.areaProvider = areaProvider;
            this.itemAreaProvider = itemAreaProvider;
            this.compartmentProvider = compartmentProvider;
            this.loadingUnitProvider = loadingUnitProvider;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
            this.schedulerService = schedulerService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(ItemArea), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("{id}/allowed-areas/{areaId}")]
        public async Task<ActionResult> CreateAllowedAreaAsync(int id, int areaId)
        {
            var result = await this.itemAreaProvider.CreateAsync(new ItemArea { AreaId = areaId, ItemId = id });
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.PutAsync), new { id = result.Entity.Id }, result.Entity);
        }

        [ProducesResponseType(typeof(ItemDetails), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost]
        public async Task<ActionResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.itemProvider.CreateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.CreateAsync), result.Entity);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{id}/allowed-areas/{areaId}")]
        public async Task<ActionResult> DeleteAllowedAreaAsync(int id, int areaId)
        {
            var result = await this.itemAreaProvider.DeleteAsync(areaId, id);

            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await this.itemProvider.DeleteAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<Item>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllAsync(
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
                    await this.itemProvider.GetAllAsync(
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                return await this.itemProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<AllowedItemArea>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/allowed-areas")]
        public async Task<ActionResult<IEnumerable<AllowedItemArea>>> GetAllowedAreasAsync(int id)
        {
            var result = await this.itemAreaProvider.GetByItemIdAsync(id);
            if (result == null)
            {
                var message = string.Format(Resources.Errors.NoEntityExists, id);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<LoadingUnit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/allowed-loading-units")]
        public async Task<ActionResult<IEnumerable<AllowedItemArea>>> GetAllowedLoadingUnitsAsync(
            int id,
            int skip = 0,
            int take = 0,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var orderByExpression = orderBy.ParseSortOptions();
                var result = await this.loadingUnitProvider.GetAllAllowedByItemIdAsync(
                    id,
                    skip,
                    take,
                    orderByExpression,
                    where,
                    search);
                if (result == null)
                {
                    var message = string.Format(Errors.NoEntityExists, id);
                    return this.NotFound(new ProblemDetails
                    {
                        Detail = message,
                        Status = StatusCodes.Status404NotFound,
                    });
                }

                return this.Ok(result);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<Area>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/areas")]
        public async Task<ActionResult<IEnumerable<Area>>> GetAreasAsync(int id)
        {
            var result = await this.areaProvider.GetByItemIdAsync(id);
            if (result == null)
            {
                var message = string.Format(Resources.Errors.NoEntityExists, id);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<Area>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/areas-with-availability")]
        public async Task<ActionResult<IEnumerable<Area>>> GetAreasWithAvailabilityAsync(int id)
        {
            var areas = await this.areaProvider.GetByItemIdAvailabilityAsync(id);

            return this.Ok(areas);
        }

        [ProducesResponseType(typeof(ItemDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<Compartment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<Compartment>>> GetCompartmentsAsync(int id)
        {
            var compartments = await this.compartmentProvider.GetByItemIdAsync(id);

            return this.Ok(compartments);
        }

        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("{id}/pick-availbility")]
        public async Task<ActionResult<double>> GetPickAvailabilityAsync(
           int id,
           [FromBody] ItemOptions putOptions)
        {
            var result = await this.schedulerService.GetPickAvailabilityAsync(id, putOptions);
            return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("{id}/put-capacity")]
        public async Task<ActionResult<double>> GetPutCapacityAsync(
            int id,
            [FromBody] ItemOptions pickOptions)
        {
            var result = await this.schedulerService.GetPutCapacityAsync(id, pickOptions);
            return !result.Success ? this.NegativeResponse(result) : this.Ok(result.Entity);
        }

        [ProducesResponseType(typeof(object[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            try
            {
                return this.Ok(await this.itemProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<SchedulerRequest>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("{id}/pick")]
        public async Task<ActionResult<IEnumerable<SchedulerRequest>>> PickAsync(
            int id,
            [FromBody] ItemOptions pickOptions)
        {
            var result = await this.schedulerService.PickItemAsync(id, pickOptions);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.PickAsync), result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<SchedulerRequest>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("{id}/put")]
        public async Task<ActionResult<SchedulerRequest>> PutAsync(
            int id,
            [FromBody] ItemOptions itemOptions)
        {
            var result = await this.schedulerService.PutItemAsync(id, itemOptions);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.CreatedAtAction(nameof(this.PutAsync), result.Entity);
        }

        [ProducesResponseType(typeof(ItemDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<ItemDetails>> UpdateAsync(ItemDetails model, int id)
        {
            if (id != model?.Id)
            {
                return this.BadRequest();
            }

            var result = await this.itemProvider.UpdateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
