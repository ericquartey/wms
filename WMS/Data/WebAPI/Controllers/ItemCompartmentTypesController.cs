using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemCompartmentTypesController :
        ControllerBase,
        ICreateController<ItemCompartmentType>
    {
        #region Fields

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Constructors

        public ItemCompartmentTypesController(
            IItemCompartmentTypeProvider itemCompartmentTypeProvider)
        {
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(201, Type = typeof(ItemCompartmentType))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<ItemCompartmentType>> CreateAsync(ItemCompartmentType model)
        {
            var result = await this.itemCompartmentTypeProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        #endregion
    }
}
