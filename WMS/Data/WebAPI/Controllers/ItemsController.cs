using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController :
        ControllerBase,
        ICreateController<ItemDetails>,
        IReadAllPagedController<Item>,
        IReadSingleController<ItemDetails, int>,
        IUpdateController<ItemDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IAreaProvider areaProvider;

        private readonly ICompartmentProvider compartmentProvider;

        private readonly IItemProvider itemProvider;

        #endregion

        #region Constructors

        public ItemsController(
            IItemProvider itemProvider,
            IAreaProvider areaProvider,
            ICompartmentProvider compartmentProvider)
        {
            this.itemProvider = itemProvider;
            this.areaProvider = areaProvider;
            this.compartmentProvider = compartmentProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(201, Type = typeof(ItemDetails))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<ItemDetails>> CreateAsync(ItemDetails model)
        {
            var result = await this.itemProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Item>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<Item>(where);

                return this.Ok(
                    await this.itemProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy,
                        whereExpression,
                        searchExpression));
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
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<Item>(where);

                return await this.itemProvider.GetAllCountAsync(
                           whereExpression,
                           searchExpression);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Area>))]
        [ProducesResponseType(404)]
        [HttpGet("{id}/areas_with_availability")]
        public async Task<ActionResult<IEnumerable<Area>>> GetAreasWithAvailabilityAsync(int id)
        {
            var areas = await this.areaProvider.GetByItemIdAvailabilityAsync(id);

            return this.Ok(areas);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Compartment>))]
        [ProducesResponseType(404)]
        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<Compartment>>> GetCompartmentsAsync(int id)
        {
            var compartments = await this.compartmentProvider.GetByItemIdAsync(id);

            return this.Ok(compartments);
        }

        [ProducesResponseType(200, Type = typeof(ItemDetails))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            return this.Ok(await this.itemProvider.GetUniqueValuesAsync(propertyName));
        }

        [ProducesResponseType(200, Type = typeof(ItemDetails))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPatch]
        public async Task<ActionResult<ItemDetails>> UpdateAsync(ItemDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.itemProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemDetails>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        private static Expression<Func<Item, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                i.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Description.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemCategoryDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.TotalAvailable.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
