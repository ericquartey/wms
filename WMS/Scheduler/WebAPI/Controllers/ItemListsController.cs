using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListsController : ControllerBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IServiceProvider serviceProvider;

        private readonly Core.IWarehouse warehouse;

        #endregion Fields

        #region Constructors

        public ItemListsController(
            IServiceProvider serviceProvider,
            ILogger<ItemListsController> logger,
            Core.IWarehouse warehouse)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.warehouse = warehouse;
        }

        #endregion Constructors

        #region Methods

        [HttpPost(nameof(Execute))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<ActionResult> Execute(ListExecutionRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var acceptedRequest = await this.warehouse.PrepareListForExecutionAsync(request.ListId, request.AreaId, request.BayId);
                if (acceptedRequest == null)
                {
                    this.logger.LogWarning($"Request of execution for list (id={request.ListId}) could not be processed.");

                    return this.UnprocessableEntity(this.ModelState);
                }

                this.logger.LogInformation($"Request of execution for list (id={request.ListId}) was accepted.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while processing the execution request for list (id={request.ListId}).");
                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }

        #endregion Methods
    }
}
