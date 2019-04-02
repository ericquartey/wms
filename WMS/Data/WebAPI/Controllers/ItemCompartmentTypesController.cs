using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemCompartmentTypesController :
        BaseController,
        ICreateController<ItemCompartmentType>
    {
        #region Fields

        private readonly IItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion

        #region Constructors

        public ItemCompartmentTypesController(
            IHubContext<SchedulerHub, ISchedulerHub> hubContext,
            IItemCompartmentTypeProvider itemCompartmentTypeProvider)
            : base(hubContext)
        {
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(ItemCompartmentType), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<ItemCompartmentType>> CreateAsync(ItemCompartmentType model)
        {
            var result = await this.itemCompartmentTypeProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(ItemCompartmentType), result.Entity.Id, HubEntityOperation.Created);

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        #endregion
    }
}
