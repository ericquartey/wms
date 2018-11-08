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

        private readonly ILogger logger;
        private readonly IHubContext<WakeupHub, IWakeupHub> hubContext;
        private readonly IServiceProvider serviceProvider;
        private const string DEFAULT_ORDERBY_FIELD = nameof(Item.Code);

        #endregion Fields

        #region Constructors

        public ItemsController(
            IServiceProvider serviceProvider,
            ILogger<ItemsController> logger,
            IHubContext<WakeupHub, IWakeupHub> hubContext)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.hubContext = hubContext;
        }

        #endregion Constructors

        #region Methods

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

        public static Expression<Func<T, TResult>> CreateSelectorExpression<T, TResult>(string propertyName)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            return (Expression<Func<T, TResult>>)Expression.Lambda(Expression.PropertyOrField(parameterExpression, propertyName),
                                                                    parameterExpression);
        }

        [HttpPost("withdraw")]
        public async Task<List<Mission>> Withdraw(Contracts.WithdrawRequest withdrawRequest)
        {
            this.logger.LogInformation($"Withdrawal request of item {withdrawRequest.ItemId} received.");

            var mission1Quantity = withdrawRequest.Quantity / 2;

            var missions = new List<Mission>
            {
#pragma warning disable IDE0009
                new Mission
                {
                    Id = 1,
                    ItemId = withdrawRequest.ItemId,
                    Quantity = mission1Quantity,
                    BayId = withdrawRequest.BayId,
                    TypeId = "PK"
                },
                new Mission
                {
                    Id = 2,
                    ItemId = withdrawRequest.ItemId,
                    Quantity = withdrawRequest.Quantity  - mission1Quantity,
                    BayId = withdrawRequest.BayId,
                    TypeId = "PK"
                }
 #pragma warning restore IDE0009
            };

            this.logger.LogInformation($"Notifying new missions.");
            await this.hubContext.Clients.All.NotifyNewMission(missions[0]);
            await this.hubContext.Clients.All.NotifyNewMission(missions[1]);

            return missions;
        }

        #endregion Methods
    }
}
