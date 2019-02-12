using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListRowsController : ControllerBase
    {
        #region Fields

        private readonly ILogger logger;

        private readonly Scheduler.Core.IWarehouse warehouse;

        #endregion

        #region Constructors

        public ItemListRowsController(
            ILogger<ItemListRowsController> logger,
            Scheduler.Core.IWarehouse warehouse)
        {
            this.logger = logger;
            this.warehouse = warehouse;
        }

        #endregion

        #region Methods

        [HttpPost(nameof(Execute))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<ActionResult> Execute(Scheduler.Core.ListRowExecutionRequest request)
        {
            if (request == null)
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var acceptedRequest = await this.warehouse.PrepareListRowForExecutionAsync(request.ListRowId, request.AreaId, request.BayId);
                if (acceptedRequest == null)
                {
                    this.logger.LogWarning($"Request of execution for list row (id={request.ListRowId}) could not be processed.");

                    return this.UnprocessableEntity(this.ModelState);
                }

                this.logger.LogInformation($"Request of execution for list row (id={request.ListRowId}) was accepted.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while processing the execution request for list row (id={request.ListRowId}).");
                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }

        #endregion
    }
}
