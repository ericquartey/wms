using System;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.WMS.Scheduler.WebAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        #region Fields

        private const string DEFAULT_ORDERBY_FIELD = nameof(Item.Code);

        private readonly IHubContext<WakeupHub, IWakeupHub> hubContext;

        private readonly ILogger logger;

        private readonly IServiceProvider serviceProvider;

        private readonly Core.IWarehouse warehouse;

        #endregion Fields

        #region Constructors

        public ItemsController(
            IServiceProvider serviceProvider,
            ILogger<ItemsController> logger,
            Core.IWarehouse warehouse,
            IHubContext<WakeupHub, IWakeupHub> hubContext)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.warehouse = warehouse;
            this.hubContext = hubContext;
        }

        #endregion Constructors

        #region Methods

        [HttpPost(nameof(Withdraw))]
        [ProducesResponseType(201, Type = typeof(Core.SchedulerRequest))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Withdraw([FromBody] Core.SchedulerRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            this.logger.LogInformation($"Withdrawal request of item {request.ItemId} received.");

            try
            {
                var acceptedRequest = await this.warehouse.WithdrawAsync(request);
                if (acceptedRequest == null)
                {
                    this.logger.LogWarning($"Withdrawal request of item {request.ItemId} could not be processed.");

                    return this.UnprocessableEntity(this.ModelState);
                }

                this.logger.LogInformation($"Withdrawal request of item {request.ItemId} accepted.");

                return this.CreatedAtAction(nameof(this.Withdraw), new { id = acceptedRequest.Id }, acceptedRequest);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while processing the withdrawal request of item {request.ItemId}.");
                return this.BadRequest(ex.Message);
            }
        }

        #endregion Methods
    }
}
