using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
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

        public static Expression<Func<T, TResult>> CreateSelectorExpression<T, TResult>(string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, TResult>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, propertyName),
                                                                    parameterExpression);
        }

        [HttpGet]
        public IEnumerable<Item> GetAll(int skip = 0, int take = int.MaxValue, string orderBy = DEFAULT_ORDERBY_FIELD)
        {
            using (var databaseContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
            {
                var orderByField = string.IsNullOrWhiteSpace(orderBy) ? DEFAULT_ORDERBY_FIELD : orderBy;
                var skipValue = skip < 0 ? 0 : skip;
                var takeValue = take < 0 ? int.MaxValue : take;

                var expression = CreateSelectorExpression<Common.DataModels.Item, object>(orderByField);

                return databaseContext.Items
                    .Skip(skipValue)
                    .Take(takeValue)
                    .OrderBy(expression)
                    .Select(i => new Item
                    {
                        Id = i.Id,
                        Code = i.Code
                    }
                    )
                    .ToList();
            }
        }

        [HttpPost("withdraw")]
        [ProducesResponseType(201, Type = typeof(SchedulerRequest))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> Withdraw([FromBody] SchedulerRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            this.logger.LogInformation($"Withdrawal request of item {request.ItemId} received.");

            try
            {
                var acceptedRequest = await this.warehouse.Withdraw(request);
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
                this.logger.LogError(ex, $"Withdrawal request of item {request.ItemId} could not be processed.");
                return this.BadRequest(this.ModelState);
            }
        }

        #endregion Methods
    }
}
