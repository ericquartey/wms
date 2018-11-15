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

                var expression = CreateSelectorExpression<Common.DataModels.Item, string>(orderByField);

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
        [ProducesResponseType(200, Type = typeof(Core.WarehouseHandlingRequest))]
        [ProducesResponseType(201, Type = typeof(Core.WarehouseHandlingRequest))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Withdraw([FromBody] Contracts.WithdrawRequest withdrawRequest)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            this.logger.LogInformation($"Withdrawal request of item {withdrawRequest.ItemId} received.");

            var acceptedRequest = await this.warehouse.Withdraw(
                withdrawRequest.ItemId,
                withdrawRequest.Quantity,
                withdrawRequest.Lot,
                withdrawRequest.RegistrationNumber,
                withdrawRequest.Sub1,
                withdrawRequest.Sub2);

            if (acceptedRequest == null)
            {
                this.logger.LogWarning($"Withdrawal request of item {withdrawRequest.ItemId} could not be processed.");

                return this.UnprocessableEntity(this.ModelState);
            }

            this.logger.LogInformation($"Withdrawal request of item {withdrawRequest.ItemId} accepted.");
            return this.CreatedAtAction(nameof(this.Withdraw), new { id = acceptedRequest.Id }, acceptedRequest);
        }

        #endregion Methods
    }
}
